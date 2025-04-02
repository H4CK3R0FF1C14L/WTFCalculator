using WTFCalculator.Models;

namespace WTFCalculator.Services
{
    public class CombinationFinderService
    {

        // Knapsack Trash
        // Uses a huge amount of memory
        #region Knapsack

        public List<Dictionary<string, int>> FindCombinations(List<Item> items, decimal targetSum)
        {
            var dp = new Dictionary<decimal, List<Dictionary<string, int>>>();
            dp[0m] = new List<Dictionary<string, int>> { new Dictionary<string, int>() };

            foreach (var item in items.OrderByDescending(i => i.Cost))
            {
                var newDp = new Dictionary<decimal, List<Dictionary<string, int>>>();

                foreach (var (sum, combinations) in dp)
                {
                    for (int q = 0; q <= item.Count!.Value; q++)
                    {
                        decimal newSum = sum + q * item.Cost!.Value;
                        if (newSum > targetSum) break;

                        foreach (var combo in combinations)
                        {
                            var newCombo = new Dictionary<string, int>(combo);
                            if (q > 0) newCombo[item.Name!] = q;

                            if (!newDp.ContainsKey(newSum))
                                newDp[newSum] = new List<Dictionary<string, int>>();

                            newDp[newSum].Add(newCombo);
                        }
                    }
                }

                foreach (var (sum, combos) in newDp)
                {
                    if (!dp.ContainsKey(sum))
                        dp[sum] = new List<Dictionary<string, int>>();

                    dp[sum].AddRange(combos);
                }
            }

            return dp.TryGetValue(targetSum, out var result) ? result : new List<Dictionary<string, int>>();
        }

        public (List<Dictionary<string, int>> Results, List<Dictionary<string, int>> CloseResults)
            FindCombinations(List<Item> items, decimal targetSum, decimal tolerance)
        {
            var dp = new Dictionary<decimal, List<Dictionary<string, int>>>();
            dp[0m] = new List<Dictionary<string, int>> { new Dictionary<string, int>() };

            foreach (var item in items.OrderByDescending(i => i.Cost))
            {
                var newDp = new Dictionary<decimal, List<Dictionary<string, int>>>();

                foreach (var (sum, combinations) in dp)
                {
                    for (int q = 0; q <= item.Count!.Value; q++)
                    {
                        decimal newSum = sum + q * item.Cost!.Value;
                        if (newSum > targetSum + tolerance) continue;

                        foreach (var combo in combinations)
                        {
                            var newCombo = new Dictionary<string, int>(combo);
                            if (q > 0) newCombo[item.Name!] = q;

                            if (!newDp.ContainsKey(newSum))
                                newDp[newSum] = new List<Dictionary<string, int>>();

                            newDp[newSum].Add(newCombo);
                        }
                    }
                }

                foreach (var (sum, combos) in newDp)
                {
                    if (!dp.ContainsKey(sum))
                        dp[sum] = new List<Dictionary<string, int>>();

                    dp[sum].AddRange(combos);
                }
            }

            var exactResults = dp.TryGetValue(targetSum, out var exact) ? exact : new List<Dictionary<string, int>>();
            var closeResults = dp.Where(kv =>
                Math.Abs(kv.Key - targetSum) <= tolerance &&
                kv.Key != targetSum)
                .SelectMany(kv => kv.Value)
                .ToList();

            return (exactResults, closeResults);
        }

        #endregion


        #region RecursiveOvershoot

        /// <summary>
        /// Finds the combinations.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="targetSum">The target sum.</param>
        /// <returns></returns>
        /*public List<Dictionary<string, int>> FindCombinations(List<Item> items, decimal targetSum)
        {
            // Sort the goods and remove the extra ones that cost more than the declared amount
            List<Item> validItems = items
                .Where(item => item.Cost.HasValue && item.Cost.Value > 0 &&             // Cost > 0
                                item.Count.HasValue && item.Count.Value > 0)            // Count > 0
                .OrderByDescending(item => item.Cost!.Value)                            // Optimization: sorting in descending order of value
                .ToList();

            // Current Quantities
            int[] currentQuantities = new int[validItems.Count];
            List<int[]> quantityResults = new List<int[]>();

            GenerateCombinations(validItems, 0, 0m, currentQuantities, quantityResults, targetSum);

            // Convert results into a convenient format
            List<Dictionary<string, int>> result = new List<Dictionary<string, int>>();
            foreach (int[]? quantities in quantityResults)
            {
                Dictionary<string, int> combination = new Dictionary<string, int>();
                for (int i = 0; i < validItems.Count; i++)
                {
                    if (quantities[i] > 0)
                    {
                        combination[validItems[i].Name!] = quantities[i];
                    }
                }
                result.Add(combination);
            }

            return result;
        }

        public (List<Dictionary<string, int>> Results, List<Dictionary<string, int>> ShitResults) FindCombinations(List<Item> items, decimal targetSum, decimal difference)
        {
            // Sort the goods and remove the extra ones that cost more than the declared amount
            List<Item> validItems = items
                .Where(item => item.Cost.HasValue && item.Cost.Value > 0 &&             // Cost > 0
                                item.Count.HasValue && item.Count.Value > 0)            // Count > 0
                .OrderByDescending(item => item.Cost!.Value)                            // Optimization: sorting in descending order of value
                .ToList();

            // Current Quantities
            int[] currentQuantities = new int[validItems.Count];
            List<int[]> quantityResults = new List<int[]>();
            List<int[]> shitQuantityResults = new List<int[]>();

            GenerateCombinations(validItems, 0, 0m, currentQuantities, quantityResults, targetSum, shitQuantityResults, difference);

            // Convert results into a convenient format
            List<Dictionary<string, int>> result = new List<Dictionary<string, int>>();
            foreach (int[]? quantities in quantityResults)
            {
                Dictionary<string, int> combination = new Dictionary<string, int>();
                for (int i = 0; i < validItems.Count; i++)
                {
                    if (quantities[i] > 0)
                    {
                        combination[validItems[i].Name!] = quantities[i];
                    }
                }
                result.Add(combination);
            }

            // Convert results into a convenient format
            List<Dictionary<string, int>> shitResult = new List<Dictionary<string, int>>();
            foreach (int[]? shitQuantities in shitQuantityResults)
            {
                Dictionary<string, int> combination = new Dictionary<string, int>();
                for (int i = 0; i < validItems.Count; i++)
                {
                    if (shitQuantities[i] > 0)
                    {
                        combination[validItems[i].Name!] = shitQuantities[i];
                    }
                }
                shitResult.Add(combination);
            }

            return (result, shitResult);
        }

        */



        /// <summary>
        /// Generates the combinations.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="index">The index.</param>
        /// <param name="currentSum">The current sum.</param>
        /// <param name="currentQuantities">The current quantities.</param>
        /// <param name="results">The results.</param>
        /// <param name="targetSum">The target sum.</param>
        private void GenerateCombinations(List<Item> items, int index, decimal currentSum, int[] currentQuantities, List<int[]> results, decimal targetSum)
        {
            // If the index value is equal to the number of items
            if (index == items.Count)
            {
                // If Current Sum is equal to Target Sum we add it to the result list
                if (currentSum == targetSum)
                {
                    results.Add(currentQuantities.ToArray());
                }
                return;
            }

            // Take Item from the array by index
            Item currentItem = items[index];

            // Check Remaining Sum by subtracting Current Sum from Target Sum, if it is less than 0 then exit the function
            decimal remaining = targetSum - currentSum;
            if (remaining < 0) return;

            // This code calculates the maximum number of items (maxQuantity) that can be purchased without exceeding the available budget and item limit.
            // remaining / costPerItem - Divide the remaining budget by the price per item to find out how many pieces can be bought in theory (without considering the Count limit).
            // Math.Min(..., Count) - We take the minimum of the number of items we can buy on a budget and the maximum allowable Count.
            int maxQuantity = Math.Min((int)(remaining / currentItem.Cost!.Value), currentItem.Count!.Value);

            // The loop goes through the possible quantities of the product
            for (int quantity = 0; quantity <= maxQuantity; quantity++)
            {
                // We get New Sum by multiplying the quantity of goods by Cost and then adding it to Current Sum
                // If New Sum is greater than Target Sum then break the loop
                decimal newSum = currentSum + quantity * currentItem.Cost!.Value;
                if (newSum > targetSum) break;

                // Writing in Current Quantities
                currentQuantities[index] = quantity;
                // Making a recursive call
                GenerateCombinations(items, index + 1, newSum, currentQuantities, results, targetSum);
            }
        }


        private void GenerateCombinations(List<Item> items, int index, decimal currentSum, int[] currentQuantities, List<int[]> results, decimal targetSum, List<int[]> shitResults, decimal difference)
        {
            // If the index value is equal to the number of items
            if (index == items.Count)
            {
                // If Current Sum is equal to Target Sum we add it to the result list
                if (currentSum == targetSum)
                {
                    results.Add(currentQuantities.ToArray());
                    return;
                }

                decimal currentDifference = targetSum - currentSum;
                if (currentDifference <= difference)
                {
                    shitResults.Add(currentQuantities.ToArray());
                }

                return;
            }

            // Take Item from the array by index
            Item currentItem = items[index];

            // Check Remaining Sum by subtracting Current Sum from Target Sum, if it is less than 0 then exit the function
            decimal remaining = targetSum - currentSum;
            if (remaining < 0) return;

            // This code calculates the maximum number of items (maxQuantity) that can be purchased without exceeding the available budget and item limit.
            // remaining / costPerItem - Divide the remaining budget by the price per item to find out how many pieces can be bought in theory (without considering the Count limit).
            // Math.Min(..., Count) - We take the minimum of the number of items we can buy on a budget and the maximum allowable Count.
            int maxQuantity = Math.Min((int)(remaining / currentItem.Cost!.Value), currentItem.Count!.Value);

            // The loop goes through the possible quantities of the product
            for (int quantity = 0; quantity <= maxQuantity; quantity++)
            {
                // We get New Sum by multiplying the quantity of goods by Cost and then adding it to Current Sum
                // If New Sum is greater than Target Sum then break the loop
                decimal newSum = currentSum + quantity * currentItem.Cost!.Value;
                if (newSum > targetSum) break;

                // Writing in Current Quantities
                currentQuantities[index] = quantity;
                // Making a recursive call
                GenerateCombinations(items, index + 1, newSum, currentQuantities, results, targetSum, shitResults, difference);
            }
        }

        #endregion
    }
}
