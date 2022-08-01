namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;


    // Double slashes represent my comments
    //// 4x slashes represent original comments
    public class DataReader
    {
        // made into a list because it's used as a list (which implements IEnumerable) to avoid typecasting
        List<ImportedObject> ImportedObjects;
        
        public void ImportAndPrintData(string fileToImport, bool printData = true)
        {
            ImportData(fileToImport);

            CalculateTheNumberOfChildren();

            // Because why do we need this bool otherwise?
            if (printData) PrintData();

            Console.ReadLine();
        }

        private void ImportData(string fileToImport)
        {
            // removed unnecessary addition of an empty object
            ImportedObjects = new List<ImportedObject>();

            var importedLines = new List<string>();

            // Good practice to wrap filestreams into "using" blocs
            using (StreamReader sr = new StreamReader(fileToImport))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    importedLines.Add(line);
                }
            }

            // fixed "i <= importedLines.Count" so we don't get index out of range exception
            for (int i = 0; i < importedLines.Count; i++)
            {
                var importedLine = importedLines[i];
                var values = importedLine.Split(';');

                // Wrapped in a try-catch bloc of code because of:
                // 1 - empty lines
                // 2 - potentially bad data
                // bad data is just ignored with "continue" and not imported in the list, ideally handle bad data in an actual project
                var importedObject = new ImportedObject();
                try
                {
                    importedObject.Type = values[0];
                    importedObject.Name = values[1];
                    importedObject.Schema = values[2];
                    importedObject.ParentName = values[3];
                    importedObject.ParentType = values[4];
                    importedObject.DataType = values[5];
                    importedObject.IsNullable = values[6];
                }
                catch (IndexOutOfRangeException ex)
                {
                    continue;
                }

                importedObject = FixImportedData(importedObject);

                // removed typecasting
                ImportedObjects.Add(importedObject);
            }
        }


        //// clear and correct imported data
        // Extracted into a separate method for convenience
        // Now runs after creating and adding all the values to the objects, avoids additional cycle through all the objects
        // removed Trim operation as it's unnecessary with space replacement
        // Set Type and Parent Type to uppercase because they have inconsistent casing in the file, also removes ToUpper from other loops that use those props
        private ImportedObject FixImportedData(ImportedObject importedObject)
        {
            importedObject.Name = importedObject.Name.Replace(" ", "").Replace(Environment.NewLine, "");
            importedObject.ParentName = importedObject.ParentName.Replace(" ", "").Replace(Environment.NewLine, "");
            importedObject.Schema = importedObject.Schema.Replace(" ", "").Replace(Environment.NewLine, "");
            importedObject.Type = importedObject.Type.Replace(" ", "").Replace(Environment.NewLine, "").ToUpper();
            importedObject.ParentType = importedObject.ParentType.Replace(" ", "").Replace(Environment.NewLine, "").ToUpper();

            return importedObject;
        }

        //// assign number of children
        // Remade both loops:
        // 1. Unnecessary use of different loops (for and foreach) for the same purpose
        // 2. Unnecessary use of ToArray() method
        // 3. Unified 2 if statements to lower the level of indentation
        private void CalculateTheNumberOfChildren()
        {
            foreach (var currentObject in ImportedObjects)
            {
                foreach (var evaluatedObject in ImportedObjects)
                {
                    if (currentObject.Type == evaluatedObject.ParentType && currentObject.Name == evaluatedObject.ParentName)
                    {
                        currentObject.NumberOfChildren++;
                    }
                }
            }
        }

        // Extracted into separate methods for more readability
        // otherwise didn't do much with those loops outside of joining if statements
        private void PrintData()
        {
            PrintDatabases();
        }

        private void PrintDatabases()
        {
            foreach (var database in ImportedObjects)
            {
                if (database.Type == "DATABASE")
                {
                    Console.WriteLine($"Database '{database.Name}' ({database.NumberOfChildren} tables)");

                    //// print all database's tables
                    PrintTables(database);
                }
            }
        }

        private void PrintTables(ImportedObject database)
        {
            foreach (var table in ImportedObjects)
            {
                if (table.ParentType == database.Type && table.ParentName == database.Name)
                {
                    Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({table.NumberOfChildren} columns)");

                    PrintColumns(table);
                }
            }
        }

        private void PrintColumns(ImportedObject table)
        {
            //// print all table's columns
            foreach (var column in ImportedObjects)
            {
                if (column.ParentType == table.Type && column.ParentName == table.Name)
                {
                    Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");
                }
            }
        }
    }

    // In a large-scale project, make these classes into separate files
    class ImportedObject : ImportedObjectBaseClass
    {
        // Removed repeated Name property
        // Fixed bracket positions
        // Added missing getters and setters for properties
        public string Schema { get; set; }
        public string ParentName { get; set; }
        public string ParentType { get; set; }
        public string DataType { get; set; }
        public string IsNullable { get; set; }
        public double NumberOfChildren { get; set; }
    }

    class ImportedObjectBaseClass
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    // Had me a bit confused at first, but there are severe issues being suppressed in this solution. Don't do that :<
}
