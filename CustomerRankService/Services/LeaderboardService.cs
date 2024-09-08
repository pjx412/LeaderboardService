using CustomerRankService.Models;
using CustomerRankService.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CustomerRankService.Services
{
    public class LeaderboardService
    {
        // Use a thread-safe dictionary to ensure that operations on the dictionary are thread-safe.
        private readonly ConcurrentDictionary<long, Customer> _customers = new ConcurrentDictionary<long, Customer>();

        // Use SortedList to automatically sort by Score in descending order,
        // while also storing the corresponding CustomerId for each Score and
        // automatically sorting them in ascending order.
        private readonly SortedList<decimal, SortedSet<long>> _sortedScores = new SortedList<decimal, SortedSet<long>>(new DescendingComparer<decimal>());

        // Use ReaderWriterLockSlim to enhance concurrency performance.
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Modify customer score
        /// </summary>
        /// <param name="customerId">customerId</param>
        /// <param name="score">score</param>
        /// <returns>new score</returns>
        public decimal UpdateScore(long customerId, decimal score)
        {
            var customer = _customers.GetOrAdd(customerId, id => new Customer { CustomerId = id, Score = 0 });

            // Acquire write lock
            _lock.EnterWriteLock();
            try
            {
                    if (customer.Score > 0)
                    {
                        _sortedScores[customer.Score] .Remove(customerId);
                        if (_sortedScores[customer.Score].Count == 0)
                        {
                            _sortedScores.Remove(customer.Score);
                        }
                    }

                    customer.Score += score;
                    if (customer.Score > 0)
                    {
                        if (!_sortedScores.ContainsKey(customer.Score))
                        {
                            _sortedScores[customer.Score] = new SortedSet<long>();
                        }
                        _sortedScores[customer.Score].Add(customerId);
                    }
                RecalculateRanks();
            }
            finally
            {
                // Release write lock
                _lock.ExitWriteLock();
            }

            return customer.Score;
        }

        public List<Customer> GetCustomersByRank(int start, int end)
        {
            //Acquire read lock to ensure data is not modified during the reading process.
            if (!_lock.IsReadLockHeld)
            {
                _lock.EnterReadLock();
            }
            try
            {
                var customersInRange = new List<Customer>();

                // Iterate through the sorted scores to find the customers within the specified range
                int currentRank = 1;
                foreach (var scoreGroup in _sortedScores)
                {
                    foreach (var customerId in scoreGroup.Value)
                    {
                        if (currentRank >= start && currentRank <= end)
                        {
                            if (_customers.TryGetValue(customerId, out var customer))
                            {
                                customersInRange.Add(customer);
                            }
                        }
                        if (currentRank > end)
                        {
                            break;
                        }
                        currentRank++;
                    }
                    if (currentRank > end)
                    {
                        break;
                    }
                }

                return customersInRange;
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public List<Customer> GetCustomersByCustomerId(long customerId, int high = 0, int low = 0)
        {
            _lock.EnterReadLock();
            try
            {
                var result = new List<Customer>();
                if (_customers.TryGetValue(customerId, out var customer))
                {
                    if (customer.Score <= 0) return result;

                    // Calculate the start and end indices based on the customer's rank
                    int start = Math.Max(1, customer.Rank - high);
                    int end =  customer.Rank + low;
                    return GetCustomersByRank(start, end);
                }

                return result;
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Update rankings for all customers
        /// </summary>
        private void RecalculateRanks()
        {
            int rank = 1;
            foreach (var scoreGroup in _sortedScores)
            {
                foreach (var customerId in scoreGroup.Value)
                {
                    _customers[customerId].Rank = rank++;
                }
            }
        }
    }
}