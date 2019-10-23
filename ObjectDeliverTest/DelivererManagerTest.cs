using NUnit.Framework;
using System.Threading.Tasks;
using ObjectDeliverer;

namespace ObjectDeliverTest
{
    public class DelivererManagerTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test1()
        {
            var deliverer = ObjectDelivererManager.CreateObjectDelivererManager();
            //deliverer.Connected.Subscribe(x => Console.WriteLine($"{x}"));
            //deliverer.DisConnected.Subscribe(x => Console.WriteLine($"{x}"));
            //deliverer.Received.Subscribe(x => Console.WriteLine($"{x.data}"));


            //await deliverer.Start(null, null, null);


            //await deliverer.Send(new byte[0]);


            await deliverer.CloseAsync();
        }
    }
}