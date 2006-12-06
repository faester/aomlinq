using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace GenDB
{
    internal class GenTable
    {
        SqlConnection cnn;

        public GenTable()
        {
            string db = GenericDB.Instance.Connection.Database;
            cnn = new SqlConnection(GenericDB.Instance.Connection.ConnectionString + ";database=" + db);
            cnn.Open();
        }

        public void Add(IBusinessObject ibo)
        {
            Translator.UpdateDBWith(ibo);
        }


        /// <summary>
        /// Returns all objects in database.
        /// If the cache contains an object with the same EntityPOID, the cached version will be used instead.
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IBusinessObject> GetAll()
        {
            if (cnn.State == System.Data.ConnectionState.Closed)
            {
                cnn.Open();
            }

            SqlCommand cmd = new SqlCommand(
                "SELECT e.EntityTypePOID, pv.EntityPOID, pv.PropertyPOID, pv.TheValue "
                + " FROM PropertyValue pv INNER JOIN Entity e "
                + " ON e.EntityPOID = pv.EntityPOID "
                + " WHERE pv.EntityPOID IN "
                + " (SELECT DISTINCT EntityPOID FROM Entity)" // "WHERE"-delen er i realiteten at udvælge de korrekte EntityPOID'er her
                + " ORDER BY EntityPOID "
                , cnn);
            SqlDataReader reader = cmd.ExecuteReader();

            Translator currentTranslator = Translator.GetCreateTranslator(typeof(object));
            long currentEntityType = currentTranslator.EntityTypePOID;
            IBusinessObject currentObject = null;
            long currentObjectID = currentTranslator.EntityTypePOID;
            bool first = true;
            bool cacheCopyNotFound = true;
            IBusinessObject cacheCopy = null;

            while (reader.Read())
            {
                long entityTypePOID, entityPOID, propertyPOID;
                string theValue;

                entityPOID = long.Parse(reader[1].ToString());
                entityTypePOID = long.Parse(reader[0].ToString());
                propertyPOID = long.Parse(reader[2].ToString());
                theValue = reader[3] == DBNull.Value ? null : reader[3].ToString();

                if (currentEntityType != entityTypePOID)
                { // Need to switch translator since type has changed
                    currentTranslator = Translator.GetTranslator(entityTypePOID);
                    currentEntityType = entityTypePOID;
                }

                if (currentObjectID != entityPOID || first) {
                    if (!first) {
                        if (cacheCopyNotFound) {
                            yield return currentObject;
                        } else {
                            yield return cacheCopy;
                        }
                    } else {
                        first = false;
                    }
                    currentObjectID = entityPOID;
                    cacheCopy = IBOCache.Instance.Get(entityPOID);
                    cacheCopyNotFound = cacheCopy == null;
                    currentObject = currentTranslator.NewObjectInstance();
                    if (cacheCopyNotFound)
                    {
                        DBTag.AssignDBTagTo(currentObject, entityPOID, IBOCache.Instance);
                    }
                }

                if (cacheCopyNotFound) {
                    currentTranslator
                        .GetPropertyValueConverter(propertyPOID)
                        .SetObjectsFieldValue(currentObject, theValue);
                }
            }

            reader.Close();

            if (cacheCopyNotFound) {
                yield return cacheCopy;
            } else {
                yield return currentObject;
            }
        }
    }
}
