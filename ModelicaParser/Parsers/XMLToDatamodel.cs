﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelicaParser.Datamodel;
using System.Xml;

namespace ModelicaParser.Parsers
{
    class XMLToDatamodel
    {
        /*
         * TODO : 
         * - e.g. list<list<T>> or list<tuple<Exp,Exp>> 
         * - alias for types 
         */
        static List<MetaModel> metamodels = new List<MetaModel>();

        static Dictionary<string, List<Connector>> targetElements;
        static Dictionary<string, Element> declaredElements;
        static string[] Basetypes = new string[] { "Boolean", "Integer", "Real", "String" };

        static void Main(string[] args)
        {
            for (int i = 2; i <= 6; i++)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(@"C:\Users\maxime\Desktop\XmlModelica\Absyn-1.9." + i + ".xml");

                targetElements = new Dictionary<string, List<Connector>>();
                declaredElements = new Dictionary<string, Element>();
                MetaModel metamodel = parseMetaModel(doc);
                metamodels.Add(metamodel);

                int count = 0;
                foreach (Element elem in metamodel.packages.ElementAt(0).elements)
                {
                    count += 1 + elem.children.Count;
                }

                Console.WriteLine("Number of elements : " + count);
                Console.WriteLine("Absyn-1.9." + i + " parsing to Datamodel sucessful");
            }
            Console.ReadKey();
        }

        static MetaModel parseMetaModel(XmlDocument doc)
        {
            XmlNode metamodelNode = doc.GetElementsByTagName("metamodel").Item(0);
            string version = metamodelNode.Attributes["version"].Value;
            MetaModel metamodel = new MetaModel(version);
            XmlNodeList children = metamodelNode.ChildNodes;
            for (int i = 0; i < children.Count; i++)
            {
                Package package = parsePackage(children[i]);
                package.metamodel = metamodel;
                metamodel.AddPackage(package);
            }

            Dictionary<string, List<Connector>>.KeyCollection targetsName = targetElements.Keys;
            foreach (string targetName in targetsName)
            {
                Element target = null;
                if (declaredElements.TryGetValue(targetName, out target))
                {
                    foreach (Connector connector in targetElements[targetName])
                    {
                        Connector clone = (Connector) connector.Clone();
                        clone.target = target;
                        target.AddTargetConnector(clone);
                    }
                }
                else
                {
                    Console.WriteLine("*** WARNING *** \t Can't find type : " + targetName);
                }
            }
            return metamodel;
        }

        static Package parsePackage(XmlNode elem)
        {
            string id = elem.Attributes["id"].Value;
            Package package = new Package(id);
            XmlNodeList children = elem.ChildNodes;
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i].Name == "uniontype")
                {
                    Element uniontype = parseUniontype(children[i]);
                    uniontype.package = package;
                    package.AddElement(uniontype);
                    declaredElements.Add(uniontype.name, uniontype);
                }
                else
                {
                    // TODO : handle type alias (possibly in ModelicaToXML)
                }
            }

            return package;
        }
        static Element parseUniontype(XmlNode elem)
        {
            string id = elem.Attributes["id"].Value;
            Element uniontype = new Element("uniontype", id);
            XmlNodeList children = elem.ChildNodes;

            for (int i = 0; i < children.Count; i++)
            {
                Element record = parseRecord(children[i]);
                record.parent = uniontype;
                uniontype.AddChildren(record);
            }

            return uniontype;
        }
        static Element parseRecord(XmlNode elem)
        {
            string id = elem.Attributes["id"].Value;
            Element record = new Element("record", id);
            XmlNodeList children = elem.ChildNodes;

            for (int i = 0; i < children.Count; i++)
            {
                XmlAttributeCollection attributes = children[i].Attributes;
                string type = attributes["type"].Value;
                string name = attributes["name"].Value;
                string maxMultiplicity = attributes["maxMultiplicity"].Value;
                string minMultiplicity = attributes["minMultiplicity"].Value;

                if (Basetypes.Contains<string>(type))
                {
                    ModelicaParser.Datamodel.Attribute attribute = new ModelicaParser.Datamodel.Attribute(type, name, maxMultiplicity, minMultiplicity);
                    attribute.parent = record;
                    record.AddAttribute(attribute);
                }
                else
                {
                    Connector c = new Connector("Association", "1", minMultiplicity + ".." + maxMultiplicity);
                    c.source = record;
                    record.AddSourceConnector(c);
                    if(!targetElements.ContainsKey(type))
                    {
                        targetElements.Add(type, new List<Connector>());
                    }
                    targetElements[type].Add(c);
                }
            }

            return record;
        }



    }
}
