using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace GenDB
{
    internal class Translator
    {
        FieldInfo[] fields;        
        Type objectType;
        IBOCache cache;

        private Translator superTranslator = null;

        private Translator() { /* empty */ }

        public Translator(Type objectType, IBOCache cache)
        {
            this.objectType = objectType;
            this.cache = cache;
            Init();
        }

        private void Init()
        {

        }
    }
}
