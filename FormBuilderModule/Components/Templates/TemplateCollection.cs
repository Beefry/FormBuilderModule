using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beefry.FormBuilder
{
    public class TemplateCollection : ICollection<Template>, IEnumerable<Template>
    {
        private List<Template> Forms;

        public TemplateCollection()
        {
            Forms = new List<Template>();
        }

        #region ICollection Definitions
        public int Count
        {
            get
            {
                return Forms.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(Template item)
        {
            Forms.Add(item);
        }

        public void Clear()
        {
            Forms.Clear();
        }

        public bool Contains(Template item)
        {
            return Forms.Contains(item);
        }

        public void CopyTo(Template[] array, int arrayIndex)
        {
            Forms.CopyTo(array, arrayIndex);
        }

        public bool Remove(Template item)
        {
            return Forms.Remove(item);
        }
        #endregion

        #region IEnumerable Definitions
        public IEnumerator<Template> GetEnumerator()
        {
            return Forms.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion
    }
}
