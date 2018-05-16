using System;
using System.Collections.Generic;
using System.Threading;

//
// Demonstrate how an input queue can be shared among a collection of competing consumers.
// This suits the scenario where consumption is slow, but can be somewhat improved by
// consuming in bulk versus singly.
//

namespace ProducerConsumers
{
    public class ProducerConsumers
    {
        private const int RUNNING = 0;
        private const int EXITING = 1;
        private List<int> _buffer = new List<int>();
        private int _state = RUNNING;
        private Thread _producer;
        private List<Thread> _consumers = new List<Thread>();

        public ProducerConsumers(int consumerCount = 1)
        {
            _producer = new Thread(Produce);
            _producer.Start();

            for(int i = 0; i < consumerCount; i++)
            {
                var t = new Thread(Consume);
                t.Start();
                _consumers.Add(t);
                Console.WriteLine("Created consumer {0}", i+1);
            }
        }

        public void Exit()
        {
            Interlocked.Exchange(ref _state, EXITING);
            _producer.Join();

            foreach (var t in _consumers)
                t.Join();
        }

        private void Produce()
        {
            int i = 0;
            while( _state == RUNNING)
            {
                _buffer.Add(i++);
                Thread.Sleep(10);
            }
        }

        private void Consume()
        {
            List<int> input = new List<int>();
            
            while (_state == RUNNING)
            {
                if (_buffer.Count > 0)
                {
                    input = Interlocked.Exchange(ref _buffer, input);
                    Console.WriteLine("Got {0}", input.Count);
                    input.Clear();
                    Thread.Sleep(100);
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var app = new ProducerConsumers( consumerCount: 3);
            Console.ReadKey();
            app.Exit();
        }
    }
}
