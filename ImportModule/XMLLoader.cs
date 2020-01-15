using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml;
using DataModel.Input;

namespace ImportModule
{
    public class XMLLoader : DataLoader
    {
        private void checkEnumValue(string criterionName, string enumID, string enumValue)
        {
            if (enumValue.Equals(""))
                throw new ImproperFileStructureException("Enum type value can not be empty. Criterion " + criterionName + ", EnumID: " +
                                                         enumID + ".");

            float output = 0;
            if (!float.TryParse(enumValue, NumberStyles.Any, CultureInfo.InvariantCulture, out output))
                throw new ImproperFileStructureException("Improper value format '" + enumValue +
                                                         "'. Value has to be floating point. Criterion " + criterionName + ", EnumID: " +
                                                         enumID + ".");
        }

        protected override void ProcessFile(string filePath)
        {
            ValidateFilePath(filePath);
            ValidateFileExtension(filePath, ".xml");

            //load XML
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            var nameAttributeId = "";
            var descriptionAttributeId = "";

            var attributes = xmlDoc.GetElementsByTagName("ATTRIBUTES");

            var objects = xmlDoc.GetElementsByTagName("OBJECTS");

            foreach (XmlNode attribute in attributes[0].ChildNodes)
            {
                var criterion = new Criterion
                    {ID = checkCriteriaIdsUniqueness(attribute.Attributes["AttrID"].Value), LinearSegments = 1};
                // two specific groups of nodes may appear in attributes - name and description
                // we don't want to save it as criterion
                var saveCriterion = true;
                var enumIdsNamesDictionary = new Dictionary<string, string>();
                var enumIdsValuesDictionary = new Dictionary<string, float>();

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
                                    enumIdsNamesDictionary.Add(enumName.Attributes["EnumID"].Value,
                                        enumName.Attributes["Value"].Value);
                            }

                            break;
                        case "NAME":
                            criterion.Name = checkCriteriaNamesUniqueness(value);
                            break;
                        case "DESCRIPTION":
                            criterion.Description = value;
                            break;
                        case "CRITERION":
                            if (value == "Rank")
                            {
                                foreach (XmlNode enumValue in attributePart)
                                {
                                    checkEnumValue(criterion.Name, enumValue.Attributes["EnumID"].Value,
                                        enumValue.Attributes["Value"].Value);
                                    enumIdsValuesDictionary.Add(enumValue.Attributes["EnumID"].Value,
                                        float.Parse(enumValue.Attributes["Value"].Value, CultureInfo.InvariantCulture));
                                }

                                criterion.CriterionDirection = "Cost";
                            }
                            else
                            {
                                // "Cost" or "Gain"
                                criterion.CriterionDirection = value == "Cost" ? "Cost" : "Gain";
                            }

                            break;
                        case "ROLE":
                            if (value == "Name")
                            {
                                saveCriterion = false;
                                nameAttributeId = criterion.ID;
                            }
                            else if (value == "Description")
                            {
                                saveCriterion = false;
                                descriptionAttributeId = criterion.ID;
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
                        foreach (var entry in enumIdsNamesDictionary)
                        {
                            var enumID = entry.Key;
                            var enumName = entry.Value;
                            var enumValue = enumIdsValuesDictionary[enumID];
                            criterion.EnumDictionary.Add(enumName, enumValue);
                        }
                    }

                    criterionList.Add(criterion);
                }
            }

            var nodeCounter = 1;

            foreach (XmlNode instance in objects[0].ChildNodes)
            {
                // check if number of all child nodes (except INFO node - which isn't about criterion) is equal to criterionList.Count
                var alternativesCountValidation = 0;
                var alternative = new Alternative();
                alternative.ID = checkAlternativesIdsUniqueness(instance.Attributes["ObjID"].Value);

                var criteriaValuesList = new ObservableCollection<CriterionValue>();

                foreach (XmlNode instancePart in instance)
                {
                    var value = instancePart.Attributes["Value"].Value;
                    var attributeID = instancePart.Attributes["AttrID"].Value;

                    if (attributeID == nameAttributeId) alternative.Name = checkAlternativesNamesUniqueness(value);
                }

                if (alternative.Name == null)
                    throw new ImproperFileStructureException("There was no alternative name provided in node " + nodeCounter +
                                                             " of OBJECTS.");

                foreach (XmlNode instancePart in instance)
                {
                    var value = instancePart.Attributes["Value"].Value;
                    var attributeID = instancePart.Attributes["AttrID"].Value;
                    if (attributeID != nameAttributeId && attributeID != descriptionAttributeId)
                    {
                        alternativesCountValidation++;
                        var criterion = criterionList.Find(element => element.ID == attributeID);

                        if (criterion == null)
                            throw new ImproperFileStructureException(
                                "Error while processing alternative " + alternative.Name + ": criterion with ID " +
                                attributeID + " does not exist.");

                        if (criterion.IsEnum)
                        {
                            criteriaValuesList.Add(new CriterionValue(criterion.Name, criterion.EnumDictionary[value]));
                        }
                        else
                        {
                            checkIfValueIsValid(value, criterion.Name, alternative.Name);
                            criteriaValuesList.Add(new CriterionValue(criterion.Name,
                                float.Parse(value, CultureInfo.InvariantCulture)));
                        }
                    }
                }

                if (alternativesCountValidation != criterionList.Count)
                    throw new ImproperFileStructureException("Error while processing alternative " + alternative.Name +
                                                             ": there are provided " + alternativesCountValidation +
                                                             " criteria values and required are " + criterionList.Count + ".");

                alternative.CriteriaValuesList = criteriaValuesList;
                alternativeList.Add(alternative);
                nodeCounter++;
            }
        }
    }
}