using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace GenDB
{
    internal class GenTable 
    {
        public void Add(IBusinessObject ibo)
        {
            Translator.UpdateDBWith(ibo);
        }

        public IEnumerable<IBusinessObject> GetAll()
        {

            SqlConnection cnn = (SqlConnection)GenericDB.Instance.Connection;
            if (cnn.State == System.Data.ConnectionState.Closed)
            {
                cnn.Open();
            }

            SqlCommand cmd = new SqlCommand(
                "SELECT e.EntityTypePOID, pv.EntityPOID, pv.PropertyPOID, pv.TheValue " 
                + " FROM PropertyValue pv INNER JOIN Entity e "
                + " ON e.EntityPOID = pv.EntityPOID ORDER BY EntityPOID", cnn);
            SqlDataReader reader = cmd.ExecuteReader();

            Translator currentTranslator = Translator.GetCreateTranslator(typeof(object));
            long currentEntityType = currentTranslator.EntityTypePOID;
            IBusinessObject currentObject = null;
            long currentObjectID = currentTranslator.EntityTypePOID;
            bool first = true;

            while (reader.Read ())
            {
                long entityTypePOID, entityPOID, propertyPOID;
                string theValue;

                entityTypePOID = long.Parse(reader[0].ToString());
                entityPOID = long.Parse(reader[1].ToString());
                propertyPOID = long.Parse(reader[2].ToString());
                theValue = reader[3].ToString();
            
                if (currentEntityType != entityTypePOID)
                { // Need to switch translator since type has changed
                    currentTranslator = Translator.GetTranslator(entityTypePOID);
                    currentEntityType = entityTypePOID;
                }
            
                if (currentObjectID != entityPOID || first)
                {
                    if (!first)
                    {
                        yield return currentObject;
                    }
                    else 
                    {
                        first = false;
                    }
                    currentObjectID = entityPOID;
                    currentObject = currentTranslator.NewObjectInstance();
                }

                currentTranslator
                    .GetPropertyValueConverter (propertyPOID)
                    .SetObjectsFieldValue(currentObject, theValue);
            }
            reader.Close();
            yield return currentObject;
        }
    }
}
