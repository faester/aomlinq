using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using GenDB;


namespace GenDB.DB
{
    class JoinPropertyIterator : IEnumerator<IBusinessObject>, IEnumerable<IBusinessObject>
    {
        SqlConnection cnn = null;
        DataContext dataContext = null;
        IExpression whereCondition = null;
        IBusinessObject current = null;
        SqlDataReader reader = null;
        IEnumerator<IEntityType> entityTypeEnumerator = null;
        IEnumerable<IProperty> properties = null;
        MSWhereStringBuilder wsb = null;
        IIBoToEntityTranslator translator = null;
        DataTable table;
        SqlCommand cmd = null;
        bool isDisposed = false;

        public JoinPropertyIterator(DataContext dataContext, IExpression whereCondition)
        {
            Console.WriteLine("Newing a " + GetType ());
            Init(dataContext, whereCondition);
        }

        ~JoinPropertyIterator()
        {
            Console.WriteLine(GetType() + " instance destructor invoked.");
            if (!isDisposed)
            {
                Dispose();
            }
            Console.WriteLine("destructor returning");
        }

        public void Dispose()
        {
            isDisposed = true;
            Console.Write(GetType().ToString() + " instance is disposing.. ");
            entityTypeEnumerator = null;
            properties = null;
            if (reader != null && !reader.IsClosed)
            {
                reader.Close();
            }
            cnn.Close();
            Console.WriteLine(", disposing done");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        public System.Collections.Generic.IEnumerator<IBusinessObject> GetEnumerator()
        {
            return this;
        }

        public IBusinessObject Current
        {
            get { return current; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            if (reader.Read())
            {
                current = Next();
                return true;
            }
            else
            {
                if (NextEntityType())
                {
                    current = Next();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        public void Reset()
        {
            throw new Exception("Not implemented");
        }

        private bool NextEntityType()
        {
            if (reader != null && !reader.IsClosed)
            {
                Console.WriteLine("Closing reader ... ");
                reader.Close();
            }
            if (!entityTypeEnumerator.MoveNext())
            {
                return false;
            }
            else
            {
                IEntityType et = entityTypeEnumerator.Current;
                properties = et.GetAllProperties;
                cmd = new SqlCommand(ConstructSqlString(properties, et), cnn);
                cmd.CommandTimeout = dataContext.CommandTimeout;
                reader = cmd.ExecuteReader();
                translator = dataContext.Translators.GetTranslator(et.EntityTypePOID);
                return true;
            }
        }

        private IBusinessObject Next()
        {
            int entityPOID = reader.GetInt32(1);
            IBusinessObject res = null;

            if (!dataContext.IBOCache.TryGet(entityPOID, out res))
            {
                res = translator.CreateInstanceOfIBusinessObject();
                res.DBIdentity = new DBIdentifier(entityPOID, true);
                this.dataContext.IBOCache.AddFromDB(res);

                int idx = 1;
                foreach (IProperty prop in properties)
                {
                    idx++;
                    int pid = prop.PropertyPOID;
                    switch (prop.MappingType)
                    {
                        case MappingType.BOOL:
                            translator.SetProperty(pid, res, reader.GetBoolean(idx)); break;
                        case MappingType.DATETIME:
                            translator.SetProperty(pid, res, new DateTime(reader.GetInt64(idx))); break;
                        case MappingType.DOUBLE:
                            translator.SetProperty(pid, res, reader.GetDouble(idx)); break;
                        case MappingType.LONG:
                            translator.SetProperty(pid, res, reader.GetInt64(idx)); break;
                        case MappingType.REFERENCE:
                            {
                                if (reader[idx] == DBNull.Value)
                                {
                                    translator.SetProperty(pid, res, null); break;
                                }
                                else
                                {
                                    translator.SetProperty(pid, res, reader.GetInt32(idx)); break;
                                }
                            }
                            break;
                        case MappingType.STRING:
                            if (reader[idx] == DBNull.Value)
                            {
                                translator.SetProperty(pid, res, null); break;
                            }
                            else
                            {
                                translator.SetProperty(pid, res, reader.GetString(idx)); break;
                            }
                        default:
                            throw new Exception("Mapping type not implemented. " + prop.MappingType);
                    } // switch
                } // foreach

            } // if EnityPOID recognized by IBOCache
            return res;
        }

        void Init(DataContext dataContext, IExpression whereCondition)
        {
            this.whereCondition = whereCondition;
            this.dataContext = dataContext;
            //cnn = ConnectionPool.NextConnection();
            cnn = new SqlConnection(dataContext.ConnectStringWithDBName);
            cnn.Open();
            wsb = new MSWhereStringBuilder(dataContext.TypeSystem);
            wsb.Visit(whereCondition);
            entityTypeEnumerator = wsb.EntityTypes.GetEnumerator();
            NextEntityType();
        }

        string ConstructSqlString(IEnumerable<IProperty> properties, IEntityType et)
        {
            string conditionWhereString = wsb.WhereStr;
            StringBuilder selectPart = new StringBuilder("SELECT e.EntityTypePOID, e.EntityPOID ");
            StringBuilder joinPart = new StringBuilder(" FROM (")
                            .Append(conditionWhereString)
                            .Append(") ew INNER JOIN Entity e ");
            joinPart.Append(" ON ew.EntityPOID = e.EntityPOID ");
            foreach (IProperty p in properties)
            {
                int propertyID = p.PropertyPOID;
                string pAlias = "p" + propertyID;
                string select = pAlias;
                switch (p.MappingType)
                {
                    case MappingType.BOOL:
                        select += ".BoolValue"; break;
                    case MappingType.DATETIME:
                    case MappingType.LONG:
                        select += ".LongValue"; break;
                    case MappingType.DOUBLE:
                        select += ".DoubleValue"; break;
                    case MappingType.REFERENCE:
                        select += ".ReferenceValue"; break;
                    case MappingType.STRING:
                        select += ".StringValue"; break;
                    default:
                        throw new Exception("Don't know how to handle mappingtype: " + p.MappingType);

                }
                selectPart.Append(", ");
                selectPart.Append(select);
                joinPart.Append(" INNER JOIN PropertyValue ");
                joinPart.Append(pAlias);
                joinPart.Append(" ON e.EntityPOID = ");
                joinPart.Append(pAlias);
                joinPart.Append(".EntityPOID AND ");
                joinPart.Append(pAlias);
                joinPart.Append(".PropertyPOID = ");
                joinPart.Append(propertyID);
            }
            joinPart.Append(" \nWHERE e.EntityTypePOID = " + et.EntityTypePOID);
            //joinPart.Append (" OPTION (LOOP JOIN) ");
            string sqlStr = selectPart.ToString() + joinPart.ToString();
            return sqlStr;
        } // foreach EntityType
    }
}
