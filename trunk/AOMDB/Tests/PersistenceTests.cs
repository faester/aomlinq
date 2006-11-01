using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using AOM;
using Persistence;

namespace Tests
{
    [TestFixture]
    public class PersistenceTests
    {
        private const string testEntityTypeName = "TestEntityType";
        EntityType et;
        PropertyType String;
        PropertyType Integer;
        Property p0;
        Property p1;
        Property p2;
        Property p3;
        Database db;
        long entityID;
        Entity e = null;
        //Entity eWithValuesSet = null;
        DBTag tag = null;

        //[TestFixtureSetUp]
        //public void FixtureSetup()
        //{
        //    EntityTypeLoader.Load();
        //}

        #region Setup and TearDown
        [SetUp]
        public void Setup()
        {
            db = Database.Instance;
            if (!EntityType.EntityTypeExists(testEntityTypeName))
            {
                et = EntityType.CreateType(testEntityTypeName, null);
                String = new PropertyType("string");
                Integer = new PropertyType("Integer");
                p0 = new Property("firstName", String);
                p1 = new Property("birthyear", Integer);
                p2 = new Property("lastName", String);
                p3 = new Property("salutation", String);
                et.AddProperty(p0);
                et.AddProperty(p1);
                et.AddProperty(p2);
                et.AddProperty(p3);
            }
            else
            {
                et = EntityType.GetType (testEntityTypeName);
            }
            e = et.New();
        }

        [TearDown]
        public void TearDown()
        {
            db = null;
            et = null;
            String = null;
            Integer = null;
            p0 = null;
            p1 = null;
            p2 = null;
            p3 = null;
        }
        #endregion

        /// <summary>
        /// Performs no real testing. Simply check
        /// that the method does not throw an Exception.
        /// </summary>
        [Test]
        public void DatabaseStore()
        {
            Assert.IsFalse(db == null, "Database object was null. Error in test setup.");
            Assert.IsFalse(e == null, "Entity objet wass null. Error in test setup.");
            db.Store(e);
            entityID = e.Id;
        }

        [Test]
        public void DatabaseRetrieve()
        {
            db.Store (e);
            entityID = e.Id;
            Assert.IsFalse(db == null, "Database == null. Error in test setup.");
            Assert.IsFalse(entityID == Entity.UNDEFINED_ID, "Error in test setup. entityID was undefined");

            Entity ecopy = db.Retrieve(entityID);
            Assert.IsTrue(ecopy.Equals(e));
        }

        [Test]
        public void EntityPOIDAssignment()
        {
            Assert.IsTrue (e.Id == Entity.UNDEFINED_ID, "Unsaved Entity instances must have Id == Entity.UNDEFINED_ID");
            db.Store(e);
            Assert.IsTrue (e.Id != Entity.UNDEFINED_ID, "Entity instances must have their ID set when they are saved to DB");
        }

        [Test]
        public void EntityTypePreservation()
        {
            db.Store (e);
            entityID = e.Id;
            Entity copy = db.Retrieve(entityID);
            Assert.IsTrue(e.Type.Id == copy.Type.Id, "EntityType of retrived object did not match type of the saved object.");
            Assert.IsTrue(e.Type.Name == copy.Type.Name, "EntityType of saved and retrieved objects did not match.");
        }

        [Test]
        public void PropertyValuePreservation()
        {
            string val = "testName987981237";
            e.SetProperty("firstName", val) ;
            db.Store (e);
            Entity copy = db.Retrieve (e.Id);
            Assert.IsTrue(e.GetPropertyValue("firstName") == val, "Property value changed during storage/retrieve operation.");
            Assert.IsTrue(copy.GetPropertyValue("firstName") == e.GetPropertyValue("firstName"), "Property value changed in retrieved Entity.");
        }
    }
}
