using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airflow_Assignment {
    [Transaction(TransactionMode.Manual)]
    public class Commands : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            var uiDocument = commandData.Application.ActiveUIDocument;
            var document = uiDocument.Document;

            try {

                Reference selectedRef = uiDocument.Selection.PickObject(ObjectType.Element, new DuctSelectionFilter(), "Please select an element to start with");
                Element duct = document.GetElement(selectedRef);
                ConnectorSet connectors = GetConnectors(duct);
                if (connectors == null || connectors.Size == 0) {
                    TaskDialog.Show("Erro", "The selected duct has no connectors.");
                    return Result.Failed;
                }

                // Calculate Airflow
                double totalAirflow = CalculateTotalAirflow(duct, document);

                
                TaskDialog.Show("Total Airflow", $"The total airflow is: {totalAirflow} L/s");
                return Result.Succeeded;
            } catch (OperationCanceledException) {
                message = "Selection canceled by user.";
                return Result.Cancelled;
            } catch (Exception ex) {
                message = $"An unexpected error has occurred: {ex.Message}";
                return Result.Failed;
            }
        }

        private double CalculateTotalAirflow(Element startElement, Document document) {
            HashSet<ElementId> visitedElements = new HashSet<ElementId>();
            double totalAirflow = RecursiveAirflowCalculation(startElement, document, visitedElements);
            return totalAirflow;
        }

        private double RecursiveAirflowCalculation(Element element, Document document, HashSet<ElementId> visitedElements) {
            if (visitedElements.Contains(element.Id)) {
                return 0;
            }

            //  Visited Element
            visitedElements.Add(element.Id);

            double totalAirflow = 0;

            // Duct or an element that may have connectors
            ConnectorSet connectors = GetConnectors(element);

            if (connectors != null) {
                foreach (Connector connector in connectors) {
                    // Verify that the connector is of the physical type before accessing IsConnected
                    if (connector.ConnectorType == ConnectorType.End || connector.ConnectorType == ConnectorType.Curve) {
                        if (connector.IsConnected) 
                        {
                            // Iterate through all related connectors
                            foreach (Connector connectedConnector in connector.AllRefs) {
                                if (connectedConnector.Owner.Id != element.Id) {
                                    Element connectedElement = document.GetElement(connectedConnector.Owner.Id);
                                    if (connectedElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctTerminal) {
                                        totalAirflow += GetAirflow(connectedElement);
                                    } else {
                                        totalAirflow += RecursiveAirflowCalculation(connectedElement, document, visitedElements);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return totalAirflow;
        }

        // Method for Getting Connectors from an Element
        private static ConnectorSet GetConnectors(Element element) {
            if (element is FamilyInstance fi && fi.MEPModel != null) {
                return fi.MEPModel.ConnectorManager.Connectors;
            }
            else if (element is MEPCurve mc) {
                return mc.ConnectorManager.Connectors;
            }
            return null;
        }

        // Method for Obtaining the Airflow of an Element in L/s
        private static double GetAirflow(Element element) {
            Parameter airflowParam = element.LookupParameter("Airflow");
            if (airflowParam != null && airflowParam.StorageType == StorageType.Double) {
                double airflowValueInternal = airflowParam.AsDouble();
                double airflowValueInLps = UnitUtils.ConvertFromInternalUnits(airflowValueInternal, UnitTypeId.LitersPerSecond);
                
                return airflowValueInLps;
            }
            return 0;
        }

        // Custom filter to allow duct selection only
        public class DuctSelectionFilter : ISelectionFilter {
            public bool AllowElement(Element elem) {
                return (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctCurves);
            }

            public bool AllowReference(Reference reference, XYZ position) {
                return false;
            }
        }

    }
}
