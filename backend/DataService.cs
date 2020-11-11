using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace data {

    public class TodoItem
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }
    }

    public interface IDataService
    {
        List<TodoItem> Get();
        void Add(string action);
    }

    public class DataService : IDataService
    {
        private List<TodoItem> items = new List<TodoItem>();
        private readonly Random _random;

        public DataService()
        {
            // Enable random service
            _random = new Random();

            // Add a first element
            TodoItem first = new TodoItem();
            first.Id = (long)(_random.NextDouble() * 1000);
            first.Name = "Urgent task";
            first.IsComplete = false;
            items.Add(first);
        }

        public void Add(string action)
        {
            TodoItem item = new TodoItem();
            item.Id = (long)(_random.NextDouble() * 1000);;
            item.Name = action;
            item.IsComplete = false;
            Console.WriteLine("adding it again");
            items.Add(item);
        }

        public List<TodoItem> Get()
        {
            Console.WriteLine("list called " + items.Count);
            return items;
        }
    }

}


