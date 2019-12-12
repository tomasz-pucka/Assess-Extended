using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using DataModel.Input;

namespace ImportModule
{
    public class UTXLoader : DataLoader
    {
        public UTXLoader() : base()
        {

        }

        override protected void ProcessFile(string filePath)
        {
            ValidateFilePath(filePath);

            try
            {
                //load XML
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);

                string descriptionAttributeName = "";

                // iterate on its nodes
                foreach (XmlNode xmlNode in xmlDoc.DocumentElement.ChildNodes)
                {
                    if (xmlNode.Name == "ATTRIBUTES")
                    {
                        foreach (XmlNode attribute in xmlNode)
                        {
                            Criterion criterion = new Criterion() { };
                            // for UTX ID and Name are the same value
                            criterion.Name = criterion.ID = attribute.Attributes["AttrID"].Value;
                            bool saveCriterion = true;
                            Dictionary<string, string> enumIdsNamesDictionary = new Dictionary<string, string>();
                            Dictionary<string, float> enumIdsValuesDictionary = new Dictionary<string, float>();

                            foreach (XmlNode attributePart in attribute)
                            {
                                var value = attributePart.Attributes["Value"].Value;

                                switch (attributePart.Name)
                                {
                                    case "TYPE":
                                        if (value == "Enum")
                                        {
                                            criterion.IsEnum = true;
                                            foreach (XmlNode enumName in attributePart)
                                            {
                                                enumIdsNamesDictionary.Add(enumName.Attributes["EnumID"].Value, enumName.Attributes["Value"].Value);
                                            }
                                        }
                                        break;
                                    case "DESCRIPTION":
                                        criterion.Description = value;
                                        break;
                                    case "CRITERION":
                                        // if value is enum type
                                        if (value == "Rank")
                                        {
                                            foreach (XmlNode enumValue in attributePart)
                                            {
                                                enumIdsValuesDictionary.Add(enumValue.Attributes["EnumID"].Value, float.Parse(enumValue.Attributes["Value"].Value, CultureInfo.InvariantCulture));
                                            }
                                            criterion.CriterionDirection = "c";
                                        }
                                        else
                                        {
                                            // "Cost" or "Gain"
                                            criterion.CriterionDirection = value == "Cost" ? "c" : "g";
                                        }
                                        break;
                                    case "ROLE":
                                        if (value == "Description")
                                        {
                                            saveCriterion = false;
                                            descriptionAttributeName = criterion.Name;
                                        }
                                        else
                                        {
                                            saveCriterion = true;
                                        }
                                        break;
                                    default:
                                        Console.WriteLine("Improper XML structure");
                                        return;
                                }
                            }

                            if (saveCriterion)
                            {
                                if (criterion.IsEnum)
                                {
                                    criterion.EnumDictionary = new Dictionary<string, float>();
                                    foreach (KeyValuePair<string, string> entry in enumIdsNamesDictionary)
                                    {
                                        string enumID = entry.Key;
                                        string enumName = entry.Value;
                                        float enumValue = enumIdsValuesDictionary[enumID];
                                        criterion.EnumDictionary.Add(enumName, enumValue);
                                    }
                                }
                                criterionList.Add(criterion);
                            }
                        }
                    }
                    else if (xmlNode.Name == "OBJECTS")
                    {
                        foreach (XmlNode instance in xmlNode)
                        {
                            Alternative alternative = new Alternative() { Name = instance.Attributes["ObjID"].Value };
                            Dictionary<Criterion, float> criteriaValuesDictionary = new Dictionary<Criterion, float>();

                            foreach (XmlNode instancePart in instance)
                            {
                                var value = instancePart.Attributes["Value"].Value;
                                var attributeName = instancePart.Attributes["AttrID"].Value;

                                if (attributeName == descriptionAttributeName)
                                {
                                    alternative.Description = value;
                                }
                                else
                                {
                                    Criterion criterion = criterionList.Find(element => element.Name == attributeName);
                                    if (criterion.IsEnum)
                                    {
                                        float enumValue = criterion.EnumDictionary[value];
                                        //so far we save only numerical value of enum in attribute
                                        criteriaValuesDictionary.Add(criterion, enumValue);
                                    }
                                    else
                                    {
                                        criteriaValuesDictionary.Add(criterion, float.Parse(value, CultureInfo.InvariantCulture));
                                    }
                                }
                            }

                            alternative.CriteriaValues = criteriaValuesDictionary;
                            alternativeList.Add(alternative);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //TODO vonshick WARNINGS
                Trace.WriteLine("Loading UTX " + filePath + " failed! " + e.Message);
            }
        }
    }
}