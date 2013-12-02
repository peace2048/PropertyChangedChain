using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Ailes.PropertyChangedChain;

namespace sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var o1 = new Sample1();
            var o2 = new Sample2();
            o1.PropertyChanged += (o, e) => Console.WriteLine("o1 changed: id=" + o1.Id);
            o2.PropertyChanged += (o, e) => Console.WriteLine("o2 changed: id=" + o2.Id);
            Console.WriteLine("--- 最初は0 ---");
            Console.WriteLine("o1.id=" + o1.Id);
            Console.WriteLine("o2.id=" + o2.Id);
            Console.WriteLine("o2.age=" + o2.Age);
            Console.WriteLine("o1.id <= 123");
            o1.Id = 123;
            Console.WriteLine("o1.id=" + o1.Id);
            Console.WriteLine("o2.id=" + o2.Id);
            Console.WriteLine("o2.age=" + o2.Age);
            Console.WriteLine("--- propertychangedchain を適用 ---");
            var m = o1.AsPropertyChangedChain();
            m.From(_ => _.Id).Select(_ => _ % 2).DistinctUntilChanged().AssignAndRaisePropertyChanged(o2, _ => _.Id);
            m.From(_ => _.Id).AssignTo(o2, _ => _.Age);
            Console.WriteLine("o1.id=" + o1.Id);
            Console.WriteLine("o2.id=" + o2.Id);
            Console.WriteLine("o2.age=" + o2.Age);
            foreach (var item in new[] { 1, 1, 2, 3, 5, 6, 7 })
            {
                Console.WriteLine("o1.id <= " + item);
                o1.Id = item;
                Console.WriteLine("o1.id=" + o1.Id);
                Console.WriteLine("o2.id=" + o2.Id);
                Console.WriteLine("o2.age=" + o2.Age);
            }
            Console.ReadLine();
        }
    }

    class Sample1 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _id;

        public int Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    var handle = this.PropertyChanged;
                    if (handle != null) handle(this, new PropertyChangedEventArgs("Id"));
                }
            }
        }
    }

    class Sample2 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int Id { get; set; }
        public int Age { get; set; }
    }
}
