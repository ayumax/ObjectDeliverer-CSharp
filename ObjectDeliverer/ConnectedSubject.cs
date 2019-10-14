using ObjectDeliverer.Protocol;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;

namespace ObjectDeliverer
{
    public class ConnectedSubject : IObservable<IProtocol>
    {
        // Subject インスタンス
        readonly Subject<IProtocol> subject = new Subject<IProtocol>();

        // Observer を登録する
        public IDisposable Subscribe(IObserver<IProtocol> observer)
        {
            return subject.Subscribe(observer);
        }

        // 各 Observer に通知する
        public void Publish(IProtocol value)
        {
            subject.OnNext(value);
        }

    }
}
