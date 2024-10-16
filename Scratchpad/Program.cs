using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Scratchpad
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Foo foo = Blah<Foo>.Create(new Foo());

            Console.WriteLine(foo.Bar());
        }
    }

    public class Foo
    {
        public string Bar() { return "bar"; }
    }

    public class Blah<T> : System.Runtime.Remoting.Proxies.RealProxy
    {
        protected readonly T _instance;

        private Blah(T instance)
            : base(typeof(T))
        {
            _instance = instance;
        }

        public static T Create(T instance)
        {
            return (T)new Blah<T>(instance).GetTransparentProxy();
        }

        public override IMessage Invoke(IMessage msg)
        {
            return msg;
            //throw new NotImplementedException();
        }
    }
}
