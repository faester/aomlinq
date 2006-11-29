//#define RECREATE_DB
using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Xml.XLinq;
using System.Data.DLinq;

namespace GenDB
{
    class Program
    {
        class AbsBO : IBusinessObject
        {
            private DBTag dBTag;
            public DBTag DBTag 
            {
                get { return dBTag;}
                set { dBTag = value; }
            }
        }

        class A : AbsBO
        { 
            public string StringParm = DateTime.Now.ToString(); 
        }
        
        class B : A 
        {
            int year;
        }
        class C : B 
        {
            string name;
        }

        static void Main(string[] args)
        {
            GenericDB genDB = GenericDB.Instance;
            genDB.Log = Console.Out;
#if RECREATE_DB
            if (genDB.DatabaseExists())
            {
                Console.WriteLine("Deleting old database.");
                genDB.DeleteDatabase();
            }
#endif
            if (!genDB.DatabaseExists())
            {
                genDB.CreateDatabase();
            }

            EntityType et0 = new EntityType() {  Name = "et0" };
            EntityType et1 = new EntityType() {  Name = "et1" };

            PropertyType ptString = new PropertyType { Name = "string" };
            PropertyType ptInt = new PropertyType { Name = "int" };


            Entity e0 = new Entity();
            e0.EntityType = et0;

            Entity e1 = new Entity();
            Entity e2 = new Entity();
            e1.EntityType = et1;
            e2.EntityType = et1;

            Property p0 = new Property{Name = "Name", PropertyType = ptString, EntityType = et1};
            Property p1 = new Property{Name = "BirthYear", PropertyType = ptInt, EntityType = et0};

            //PropertyValue pv0  = new PropertyValue { Property = p0, TheValue = "Morten", Entity = e1 };

            PropertyValue pv0 = new PropertyValue { TheValue = "Morten", Property = p0, Entity = e1 };

            Inheritance i = new Inheritance();
            i.SuperEntityType = et0;
            i.SubEntityType = et1;
            
            genDB.Entities.Add (e0);
            genDB.Entities.Add (e1);
            genDB.Entities.Add (e2);
            genDB.PropertyValues.Add(pv0);
            //genDB.PropertyTypes.Add (ptString);
            //genDB.PropertyTypes.Add (ptInt);
            genDB.SubmitChanges();
            
            var z = from e in genDB.Entities 
                    where e.EntityPOID == 0
                    select e;

            var q = from es in genDB.PropertyValues 
                    //where es.EntityType == et1
                    select es;

            foreach (var enumer in q)
            {
                Console.WriteLine (enumer);
            }

            Translator t = Translator.GetTranslator (typeof(C));

            genDB.SubmitChanges();

            Console.WriteLine("Press Return to end..");
            Console.ReadLine();
        }
    }
}
