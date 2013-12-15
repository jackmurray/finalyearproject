using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibCommon;

namespace SpeakerController
{
    class ActiveReceiverManager : IEnumerable<Receiver>
    {
        private ListView lv;
        private List<Receiver> list = new List<Receiver>();

        public ActiveReceiverManager(ListView lv)
        {
            this.lv = lv;
        }

        public IEnumerator<Receiver> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Receiver this[int index]
        {
            get { return this.list[index]; }
        }

        public void Add(Receiver r)
        {
            this.list.Add(r);
            this.Refresh();
        }

        public void RemoveAt(int index)
        {
            this.list.RemoveAt(index);
            this.Refresh();
        }

        private void Refresh()
        {
            this.lv.Clear();
            foreach (Receiver r in this.list)
            {
                this.lv.Items.Add(new ListViewItem(r.Address.ToString()));
            }
        }
    }
}
