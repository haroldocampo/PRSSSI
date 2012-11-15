using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.IO;
using System.Data;
using Infrastructure.Helpers.Extensions;

namespace PrsDataMigration {
    class Program {
        static void Main(string[] args) {
            PrsEntities db = new PrsEntities();
            string fileName = @"C:\Users\HOCAMPO\Desktop\Sonic Steel\Templates\database.xlsx";
            string fileName2 = @"C:\Users\HOCAMPO\Desktop\Sonic Steel\Templates\costobjectivesmigration.xlsx";
            string fileName3 = @"C:\Users\HOCAMPO\Desktop\Sonic Steel\Templates\items.xlsx";
            var connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}; Extended Properties=Excel 12.0;", fileName);

            var adapter = new OleDbDataAdapter("SELECT * FROM [vendor name$]", connectionString);
            var ds = new DataSet();

            //Commented
            #region Migrate Vendors

            //adapter.Fill(ds, "Vendors");

            //DataTable table = ds.Tables["Vendors"];

            //int VendorCount = 0;

            //foreach (var data in table.AsEnumerable()) {
            //    if (data.Field<string>("Vendor Name") == null || data.Field<string>("Vendor Name") == "") {
            //        Console.WriteLine("Null Row");
            //        continue;
            //    }

            //    var NewVendor = new Vendor();
            //    NewVendor.Name = data.Field<string>("Vendor Name");
            //    NewVendor.TIN = data.Field<string>("TIN");
            //    NewVendor.Terms = data.Field<string>("TERMS");
            //    NewVendor.Address = data.Field<string>("Address");
            //    NewVendor.Telephone = data.Field<string>("TelNo");
            //    NewVendor.FaxNumber = data.Field<string>("FaxNo");
            //    NewVendor.ContactPerson = data.Field<string>("Contact Person");
            //    db.Vendors.AddObject(NewVendor);
            //    db.SaveChanges();
            //    VendorCount++;
            //    Console.WriteLine("Vendor: {0}".WithTokens(data.Field<string>("Vendor Name")));
            //    Console.WriteLine("Number of Vendors Added: " + VendorCount);
            //}

            #endregion

            //Commented
            #region Departments

            //adapter = new OleDbDataAdapter("SELECT * FROM [departments$]", connectionString);

            //adapter.Fill(ds, "Departments");

            //DataTable tableDepartments = ds.Tables["Departments"];

            //int currentCompanyId = 0;

            //int DepartmentCount = 0;

            //foreach (var data in tableDepartments.AsEnumerable()) {
            //    if (data.Field<string>("Company") == "SONIC STEEL INDUSTRIES, INC.") {
            //        currentCompanyId = 100000;
            //        Console.WriteLine("Company is now SONIC");
            //    }

            //    if (data.Field<string>("Company") == "UNITED STEEL TECHNOLOGY INT'L. CORP.") {
            //        currentCompanyId = 400000;
            //        Console.WriteLine("Company is now STEELTECH");
            //    }

            //    if (data.Field<string>("Company") == "SOMICO STEEL MILL CORPORATION") {
            //        currentCompanyId = 700000;
            //        Console.WriteLine("Company is now SOMICO");
            //    }

            //    if (data.Field<string>("Departments") == null) {
            //        Console.WriteLine("--------------Null Row--------------");
            //        continue;
            //    }

            //    Department newDepartment = new Department()
            //    {
            //        CompanyId = currentCompanyId,
            //        Name = data.Field<string>("Departments")
            //    };
            //    DepartmentCount++;
            //    Console.WriteLine("Department \"" + data.Field<string>("Departments") + "\" has been added");

            //    db.Departments.AddObject(newDepartment);
            //    db.SaveChanges();
            //}
            //Console.WriteLine(DepartmentCount + " departments has been added.");

            #endregion

            //Commented
            #region UOMs
            //adapter = new OleDbDataAdapter("SELECT * FROM [uom$]", connectionString);

            //adapter.Fill(ds, "UOMs");

            //DataTable tableUOMs = ds.Tables["UOMs"];

            //int UOMCount = 0;

            //foreach (var data in tableUOMs.AsEnumerable()) {

            //    if (data.Field<string>("UOM") == null) {
            //        Console.WriteLine("--------------Null Row--------------");
            //        continue;
            //    }

            //    UnitOfMeasurement newUOM = new UnitOfMeasurement()
            //    {
            //        Name = data.Field<string>("UOM")
            //    };
            //    UOMCount++;
            //    Console.WriteLine("UOM \"" + data.Field<string>("UOM") + "\" has been added");

            //    db.UnitOfMeasurements.AddObject(newUOM);
            //    db.SaveChanges();
            //}
            //Console.WriteLine(UOMCount + " UOMs has been added.");
            #endregion

            //Commented
            #region Cost Objectives - Sonic Steel

            //connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}; Extended Properties=Excel 12.0;", fileName2);

            //adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", connectionString);

            //adapter.Fill(ds, "CostObjSonic");

            //DataTable tableCostObj = ds.Tables["CostObjSonic"];

            //int costObjCount = 0;
            //string CategoryName = "";

            //CostObjectiveCategory dbCateg = null;
            //foreach (var data in tableCostObj.AsEnumerable()) {

            //    if (data.Field<string>("Category") != null) {
            //        CategoryName = data.Field<string>("Category");
            //        dbCateg = null;
            //        //If Category is already existing use it
            //        if (db.CostObjectiveCategories.Where(x => x.Name == CategoryName).Any()) {
            //            dbCateg = db.CostObjectiveCategories.Where(x => x.Name == CategoryName).SingleOrDefault();
            //        }
            //        else { // Else Create a new one
            //            dbCateg = new CostObjectiveCategory() { Name = CategoryName };
            //            db.CostObjectiveCategories.AddObject(dbCateg);
            //            db.SaveChanges();
            //        }
            //        Console.WriteLine("Category changed to " + CategoryName);
            //    }

            //    if (!data.Field<string>("Name").IsNullOrEmpty()) {
            //        CostObjective newCO = new CostObjective()
            //        {
            //            Name = data.Field<string>("Name"),
            //            CategoryId = dbCateg.Id,
            //            CompanyId = 100000
            //        };

            //        costObjCount++;

            //        db.CostObjectives.AddObject(newCO);
            //        db.SaveChanges();
            //        Console.WriteLine("Cost Objective \"" + data.Field<string>("Name") + "\" has been added");
            //    }
            //}
            //Console.WriteLine(costObjCount + " Cost Objectives has been added.");

            #endregion

            //Commented
            #region Cost Objectives - Somico

            //ds = new DataSet();

            //connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}; Extended Properties=Excel 12.0;", fileName);

            //adapter = new OleDbDataAdapter("SELECT * FROM [objectivessomico$]", connectionString);

            //adapter.Fill(ds, "CostObjSomico");

            //DataTable tableCostObj2 = ds.Tables["CostObjSomico"];

            //int costObjCount2 = 0;
            //string CategoryName2 = "";

            //CostObjectiveCategory dbCateg2 = null;
            //foreach (var data in tableCostObj2.AsEnumerable()) {

            //    if (data.Field<string>("Category") != null) {
            //        CategoryName2 = data.Field<string>("Category");
            //        dbCateg2 = null;
            //        // If Category is already existing use it
            //        if (db.CostObjectiveCategories.Where(x => x.Name == CategoryName2).Any()) {
            //            dbCateg2 = db.CostObjectiveCategories.Where(x => x.Name == CategoryName2).SingleOrDefault();
            //        }
            //        else { // Else Create a new one
            //            dbCateg2 = new CostObjectiveCategory() { Name = CategoryName2 };
            //            db.CostObjectiveCategories.AddObject(dbCateg2);
            //            db.SaveChanges();
            //        }
            //        Console.WriteLine("Category changed to " + CategoryName2);
            //    }

            //    if (!data.Field<string>("Name").IsNullOrEmpty()) {
            //        CostObjective newCO = new CostObjective()
            //        {
            //            Name = data.Field<string>("Name"),
            //            CategoryId = dbCateg2.Id,
            //            CompanyId = 700000
            //        };

            //        costObjCount2++;

            //        db.CostObjectives.AddObject(newCO);
            //        db.SaveChanges();
            //        Console.WriteLine("Cost Objective \"" + data.Field<string>("Name") + "\" has been added");
            //    }
            //}
            //Console.WriteLine(costObjCount2 + " Cost Objectives has been added.");

            #endregion

            #region Items

            //connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}; Extended Properties=Excel 12.0;", fileName3);

            //List<string> lstCategories = new List<string>();
            //lstCategories.Add("computer eqpt, parts & supplies");
            //lstCategories.Add("electrical supplies");
            //lstCategories.Add("fuel, lubricants");
            //lstCategories.Add("hardware supplies");
            //lstCategories.Add("machine parts");
            //lstCategories.Add("mechanical parts & supplies");
            //lstCategories.Add("office furnitures & fixtures");
            //lstCategories.Add("office supplies");
            //lstCategories.Add("paint-ink supplies");
            //lstCategories.Add("medicine supplies");
            //lstCategories.Add("safety supplies");
            //lstCategories.Add("services");
            //lstCategories.Add("tools");
            //lstCategories.Add("automotive parts");
            //lstCategories.Add("steel");
            //lstCategories.Add("vehicle");

            //foreach (var ItemCategory in lstCategories) {
            //    adapter = new OleDbDataAdapter("SELECT * FROM [{0}$]".WithTokens(ItemCategory), connectionString);

            //    DataSet ds2 = new DataSet();

            //    adapter.Fill(ds2, "Items");

            //    DataTable tableItems = ds2.Tables["Items"];

            //    string BrandName = "Generic";

            //    int itemscount = 0;

            //    ItemBrand brand = null;
            //    int whiteSpaceCount = 0;
            //    foreach (var data in tableItems.AsEnumerable()) {
            //        if (data.Field<string>("Item").IsNullOrEmpty()) {
            //            whiteSpaceCount++;
            //            continue;
            //        }

            //        if (whiteSpaceCount > 1000) {
            //            whiteSpaceCount = 0;
            //            break;
            //        }

            //        if (!data.Field<string>("Brand").IsNullOrEmpty()) {
            //            BrandName = data.Field<string>("Brand");
            //            BrandName.ToUpper();
            //            brand = null;
            //            // If Category is already existing use it
            //            if (db.ItemBrands.Where(x => x.Name == BrandName).Any()) {
            //                brand = db.ItemBrands.Where(x => x.Name == BrandName).SingleOrDefault();
            //            }
            //            else { // Else Create a new one
            //                brand = new ItemBrand() { Name = BrandName };
            //                db.ItemBrands.AddObject(brand);
            //                db.SaveChanges();
            //            }
            //            Console.WriteLine("Category changed to " + BrandName);
            //        }
            //        else {
            //            BrandName = "Generic";
            //            brand = db.ItemBrands.Where(x => x.Id == 3559).SingleOrDefault();
            //            Console.WriteLine("Category changed to " + BrandName);
            //        }

            //        if (!data.Field<string>("Item").IsNullOrEmpty()) {
            //            Item newCO = new Item()
            //            {
            //                Id = Guid.NewGuid(),
            //                Description = data.Field<string>("Item"),
            //                CodeSize = data.Field<string>("Code/Size") == null ? "" : data.Field<string>("Code/Size"),
            //                ItemBrandId = brand.Id,
            //                ItemUOMId = 134,
            //                is_active = true,
            //                date_created = DateTime.Now
            //            };
            //            itemscount++;

            //            db.Items.AddObject(newCO);
            //            db.SaveChanges();
            //            Console.WriteLine("Item " + data.Field<string>("Item") + " has been added");
            //        }
            //    }
            //    tableItems.Dispose();
            //    Console.WriteLine(itemscount + " Items has been added.");
            //    whiteSpaceCount = 0;
            //}

            #endregion

            Console.WriteLine("\n\n\nMigration Successful. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
