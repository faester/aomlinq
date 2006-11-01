using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using AOM;

namespace Tests
{
    [TestFixture]
    public class AOMTests
    {
        EntityType Object;
        EntityType Person;
        EntityType Student;
        EntityType Employee;
        PropertyType String;
        PropertyType Int;

        [TestFixtureSetUp]
        public void SetupTestFixture()
        {
            Object = EntityType.CreateType("AOMTests.Object", null);
            Person = EntityType.CreateType("AOMTests.Person", Object);
            Student = EntityType.CreateType("AOMTests.Student", Person);
            Employee = EntityType.CreateType("AOMTests.Employee", Person);
            String = new PropertyType("string");
            Int = new PropertyType("int");

            Property name = new Property("Name", String);
            Property birthYear = new Property ("BirthYear", Int);
            Property major = new Property("Major", String);
            Property Salary = new Property("Salary", String);

            Person.AddProperty(name).AddProperty(birthYear);
            Student.AddProperty(major);
            Employee.AddProperty (Salary);
        }

        [TestFixtureTearDown]
        public void TearDownTestFixture()
        {
            Object = null;
            Person = null;
            Student = null;
            Employee = null;
        }

        [Test]
        public void CanRetrieveSavedTypes()
        {
            EntityType et = EntityType.GetType("AOMTests.Object");
            Assert.IsTrue(object.ReferenceEquals(et, Object));
        }

        [Test]
        public void TestTypeExists()
        {
            Assert.IsTrue (EntityType.EntityTypeExists ("AOMTests.Object"));
            Assert.IsFalse (EntityType.EntityTypeExists ("THISTYPEDOESNOTEXIST"));
        }

        [Test, ExpectedException("AOM.UnknownPropertyException")]
        public void ExceptionOnUnknownParameterRetrieve(){
            EntityType et = EntityType.CreateType ("TestType", null);
            Entity e = et.New();
            string s = e.GetPropertyValue("UnknownProperty");
        }
        [Test, ExpectedException("AOM.UnknownPropertyException")]
        public void ExceptionOnUnknownParameterAssignment(){
            EntityType et = EntityType.CreateType ("TestType", null);
            Entity e = et.New();
            e.SetProperty ("UnknownProperty", "Failure");
        }

        [Test]
        public void PropertyAssignment()
        {
            string nameValue = "Mr. Tester";
            string byVal = "1978";

            Entity person = Person.New();
            person.SetProperty("Name", nameValue);
            person.SetProperty("BirthYear", byVal);

            Assert.AreEqual(nameValue, person.GetPropertyValue("Name"));
            Assert.AreEqual(byVal, person.GetPropertyValue("BirthYear"));
        }

        [Test]
        public void AssignmentToSuperClassProperty()
        {
            string nameValue = "Mr. Tester";
            Entity emp = Employee.New();
            emp.SetProperty("Name", nameValue);
            Assert.AreEqual(nameValue, emp.GetPropertyValue("Name"));
        }
    }
}
