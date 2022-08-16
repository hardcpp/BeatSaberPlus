#if CP_SDK_UNITY
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CP_SDK.Unity.OpenType
{
    internal class OpenTypeCollection : IEnumerable<OpenTypeFont>
    {
        private readonly CollectionHeader header;
        private OpenTypeFont[] fonts;
        private readonly bool lazy;

        internal OpenTypeCollectionReader Reader { get; }

        internal OpenTypeCollection(OpenTypeCollectionReader reader, bool lazyLoad = true) : this(reader.ReadCollectionHeader(), reader, lazyLoad)
        {
        }
        internal OpenTypeCollection(CollectionHeader header, OpenTypeCollectionReader reader, bool lazyLoad = true)
        {
            this.header = header;
            lazy = lazyLoad;
            if (lazyLoad)
                Reader = reader;
            else
                LoadAll(reader);
        }

        private void LoadAll(OpenTypeCollectionReader reader)
        {
            fonts = ReadFonts(reader);
        }

        internal IEnumerable<OpenTypeFont> Fonts
        {
            get
            {
                if (fonts == null)
                    fonts = ReadFonts(Reader);

                return fonts;
            }
        }

        private OpenTypeFont[] ReadFonts(OpenTypeCollectionReader reader)
            => reader.ReadFonts(header, lazy);

        public IEnumerator<OpenTypeFont> GetEnumerator()
            => Fonts.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => Fonts.GetEnumerator();
    }
}
#endif
