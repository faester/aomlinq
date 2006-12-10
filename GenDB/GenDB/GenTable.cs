using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace GenDB
{
    /*
     * Kan persisteres:
     *  - Objekterne skal implementere IBusinessObject og new() (Dette skal sikres af generisk deklarering på public Table<T> )
     *      - Kun felter med public getter og setter persisteres
     *          - Primitive felter persisteres.
     *          - Felter af referencetype persisteres kun, hvis de implementerer IBusinessObject
     *      - Ønskes et felt, der opfylder ovenstående, ikke persisteret kan det annoteres med [Volatile]
     *      - Statiske felter persisteres ikke
     */
    internal class GenTable
    {
        SqlConnection cnn;
        const string ePOID = "ePOID"; // Alias for entityPOID in select queries
        const string sTHEVALUE = "TheValue"; // Name of value attribute in PropertyValue table
        const string sPV = "pv"; // Prefix for property value table name alias
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

            Translator currentTranslator = Translator.GetTranslator(typeof(object));
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

        /// <summary>
        /// Returns all instances of specific type.
        /// entityTypePOID must exist.
        /// Only considers in-database objects. (For now, submit before invoking)
        /// </summary>
        /// <param name="entityTypePOID"></param>
        /// <returns></returns>
        public IEnumerable<IBusinessObject> GetAll(EntityTypeDL et) 
        {
            Translator t = Translator.GetTranslator(et.EntityTypePOID);
            string sqlSel = SQLSelectStrAllOfType(t);
            if (cnn.State != ConnectionState.Open) { cnn.Open(); }

            Console.WriteLine(sqlSel);

            IEnumerable<Converter> converters = t.AllConverters;

            SqlCommand cmd = new SqlCommand(sqlSel, cnn);

            SqlDataReader reader = cmd.ExecuteReader();

            IBusinessObject result = null;

            while(reader.Read ())
            {
                long entityPOID = long.Parse (reader[ePOID].ToString());
                result = IBOCache.Instance.Get (entityPOID);
                if (result != null)
                {
                    yield return result;
                }
                else 
                {
                    result = t.NewObjectInstance();
                    foreach (Converter c in converters)
                    {
                        string value = reader["p" + c.Property.PropertyPOID].ToString();
                        c.SetObjectsFieldValue(result, value);
                    }
                    yield return result;
                }
            }

            if (!reader.IsClosed) { reader.Close(); }
        }

        
        internal string SQLSelectStrAllOfType(Translator t)
        {
            var converters = from conv in t.AllConverters 
                             select conv;

            StringBuilder selectStr = new StringBuilder("SELECT ");
            StringBuilder fromStr = new StringBuilder("\n FROM ");
            StringBuilder whereStr = new StringBuilder ("\n WHERE (");
            bool first = true;
            Converter previousConverter = null;

            foreach (Converter currentConverter in converters )
            {
                if (!first) { 
                    fromStr.Append("\n\t INNER JOIN "); 
                    selectStr.Append(", ");
                    whereStr.Append (" AND ");
                }
                else
                {
                    selectStr.Append("\n\t ")
                             .Append(sPV)
                             .Append(currentConverter.Property.PropertyPOID)
                             .Append(".EntityPOID ")
                             .Append(ePOID)
                             .Append (", ");
                }

                selectStr.Append("\n\t ")
                         .Append(sPV)
                         .Append(currentConverter.Property.PropertyPOID)
                         .Append (".")
                         .Append (sTHEVALUE)
                         .Append (" p")
                         .Append (currentConverter.Property.PropertyPOID);

                fromStr .Append ("PropertyValue ")
                        .Append(sPV)
                        .Append(currentConverter.Property.PropertyPOID);

                whereStr .Append ("\n\t ")
                         .Append (sPV)
                         .Append (currentConverter.Property.PropertyPOID)
                         .Append (".PropertyPOID = ")
                         .Append (currentConverter.Property.PropertyPOID);

                if (!first) {
                    fromStr.Append("\n\t\t ON ")
                            .Append(sPV)
                            .Append(previousConverter.Property.PropertyPOID)
                            .Append(".EntityPOID = ")
                            .Append(sPV)
                            .Append (currentConverter.Property.PropertyPOID)
                            .Append (".EntityPOID ");
                }
                else
                {
                    first = false;
                }
                previousConverter = currentConverter;
            }
            whereStr.Append ("\n)");

            selectStr.Append (fromStr);
            selectStr.Append (whereStr);

            return selectStr.ToString();
        }
    }
}
