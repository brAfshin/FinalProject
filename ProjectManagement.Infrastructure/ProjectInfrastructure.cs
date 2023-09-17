using System.Data;
using Microsoft.Graph;
using ProjectManagement.Application.Contracts.Class;
using ProjectManagement.Domain;
using ClosedXML.Excel;
using BodyType = Microsoft.Graph.BodyType;
using Group = ProjectManagement.Domain.Group;
using MemoryStream = System.IO.MemoryStream;

namespace ProjectManagement.Infrastructure
{
    public class ProjectInfrastructure : IProjectInfrastructure
    {
        private readonly GraphServiceClient _graphServiceClient;

        public ProjectInfrastructure(GraphServiceClient graphServiceClient)
        {
            _graphServiceClient = graphServiceClient;
        }

        public async Task<string> CreateFolder(string name)
        {
            // Create Graph API Request
            var requestBody = new DriveItem
            {
                Name = name,
                Folder = new Folder
                {
                },
                AdditionalData = new Dictionary<string, object>
                {
                    {
                        "@microsoft.graph.conflictBehavior", "rename"
                    },
                },
            };

            // Send Request
            var result = await _graphServiceClient.Me.Drive.Root.Children.Request().AddAsync(requestBody);
            return result.Id;

        }

        public async Task Delete(string path)
        {
            // Send Graph API Request
            await _graphServiceClient.Me.Drive.Items[path]
                .Request()
                .DeleteAsync();
        }

        public async Task<Stream> OpenFile(string path)
        {
            // Send Graph API Request
            return await _graphServiceClient
                .Me.Drive.Items[path].Content
                .Request()
                .GetAsync();
        }

        public async Task<string> GetItemId(string path)
        {
            // Adding / at the beginning of the path
            var destination = "/" + path;
            try
            {
                // Send Request and retrieve Id
                var drive = await _graphServiceClient.Me.Drive.Root.ItemWithPath(destination).Request().GetAsync();
                var id = drive.Id;
                return id;
            }
            catch
            {
                return string.Empty;
            }

        }

        public async Task<List<Group>> GetGroups(string path)
        {
            // Get the list of files in the database folder
            var result = await _graphServiceClient.Me.Drive.Items[path].Children
                .Request()
                .Select(file => new
                {
                    file.Id,
                    file.Name,

                })
                .GetAsync();
            var classes = new List<Group>();
            foreach (var item in result)
            {
                // Split the name of the each file with - . Create a new Group based on the result.
                var value = item.Name.Split("-");
                classes.Add(new Group(System.IO.Path.GetFileNameWithoutExtension(value[1]), value[0], item.Id));
            }

            return classes;
        }

        public async Task SendEmail(Email email)
        {
            // Get the list of available emails
            var recipients = email.Recipients.Select(x => new Recipient
            {
                EmailAddress = new EmailAddress()
                {
                    Address = x
                }
            }).ToList();

            // Create a new message request
            Message message = new()
            {
                Subject = email.Subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Text,
                    Content = email.Body
                },
                ToRecipients = recipients

            };

            bool saveToSentItems = true;

            // Send request to Graph API
            await _graphServiceClient.Users[email.Sender]
                .SendMail(message, saveToSentItems)
                .Request()
                .PostAsync();
        }

        public async Task<Stream> UpdateTemplate(CopyMark command)
        {
            // Open Excel file and the corresponding sheet
            var file = await OpenFile(command.DbPath);
            var sourceBook = new XLWorkbook(file);
            var sourceSheet = sourceBook.Worksheet(command.Module);

            // Retrieve all tables then select the required table
            var tables = sourceSheet.Tables;
            var sourceTable = tables.FirstOrDefault(x => x.Name.Contains(command.Table)).AsNativeDataTable();

            // Get all students from the database file. It includes the Id column and the column which we want to get data from it.
            var sourceItems = new List<UpdateItem>();
            foreach (DataRow row in sourceTable.Rows)
            {
                sourceItems.Add(new UpdateItem()
                {
                    Id = row["id"].ToString(),
                    Value = row[command.Select].ToString()
                });
            }

            // Open the report file
            var path = await GetItemId(command.ReportPath);
            var tempFile = await OpenFile(path);
            var workbook = new XLWorkbook(tempFile);
            var worksheet = workbook.Worksheet(1);

            // Find to total number of rows
            var rowUsed = worksheet.LastRowUsed().RowNumber();


            // Each row is checked to determine if there is a value for column Id.
            // If there is a value for it, it change the selected column value for it according to the source value
            for (var row = command.StartRowId ; row < rowUsed + 1; row++)
            {
                var id = worksheet.Cell(row, command.ColId).Value.ToString().Standard();
                if (!string.IsNullOrEmpty(id))
                {
                    var value = sourceItems.FirstOrDefault(x => x.Id == id);
                    if (value!= null)
                    {
                        var item = value.Value;
                        worksheet.Cell(row,command.ColMark).Value = item;
                    }
                }
            }

            // Saving the report in a new file
            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream;


        }

        public async Task<Stream> CreateReport(CreateReport command)
        {
            // Open Excel file and the corresponding sheet
            var file = await OpenFile(command.Path);
            var workbook = new XLWorkbook(file);
            var worksheet = workbook.Worksheet(command.Module);
            // Retrieve all tables then select the required table
            var tables = worksheet.Tables;
            var infoTable = tables.FirstOrDefault(x => x.Name.Contains("info")).AsNativeDataTable();

            //In order to merge tables according to Id, we must ensure that each table has an Id.
            //Since there is a possibility that the user has already selected the Id column for info table,
            //we should check it first. We manually add id to other tables.
            var infoHasId = command.Info.Contains("Id");
            if (!infoHasId)
                command.Info.Add("Id");
            command.Mark.Add("Id");
            command.Attendance.Add("Id");
            command.Checkpoint.Add("Id");

            // Creating a new data base based on the items selected by the user from all tables.
            // Set Id as primary key for all tables.
            DataTable dtInfo = new DataView(infoTable).ToTable(false, command.Info.ToArray());
            dtInfo.PrimaryKey = new DataColumn[] { dtInfo.Columns["Id"] };

            var markTable = tables.FirstOrDefault(x => x.Name.Contains("mark")).AsNativeDataTable();
            DataTable dtMark = new DataView(markTable).ToTable(false, command.Mark.ToArray());
            dtMark.PrimaryKey = new DataColumn[] { dtMark.Columns["Id"] };

            var attendanceTable = tables.FirstOrDefault(x => x.Name.Contains("attendance")).AsNativeDataTable();
            DataTable dtAttendance = new DataView(attendanceTable).ToTable(false, command.Attendance.ToArray());
            dtAttendance.PrimaryKey = new DataColumn[] { dtAttendance.Columns["Id"] };


            var checkpointTable = tables.FirstOrDefault(x => x.Name.Contains("checkpoint")).AsNativeDataTable();
            DataTable dtCheckpoint = new DataView(checkpointTable).ToTable(false, command.Checkpoint.ToArray());
            dtCheckpoint.PrimaryKey = new DataColumn[] { dtCheckpoint.Columns["Id"] };

            // Merging all tables according the primary key
            dtInfo.Merge(dtMark,false);
            dtInfo.Merge(dtAttendance, false);
            dtInfo.Merge(dtCheckpoint, false);
            dtInfo.PrimaryKey = null;
            if (!infoHasId)
            {
                dtInfo.Columns.Remove("Id");
            }

            // Create a new file and save the Excel file on it.
            MemoryStream result = new MemoryStream();
            var resultBook = new XLWorkbook();
            var resultSheet = resultBook.AddWorksheet("report");
            resultSheet.Cell(1, 1).InsertTable(dtInfo);

            resultBook.SaveAs(result);
            return result;
        }

        public async Task CreateFile(string path, string name)
        {
            // Create a new excel and save it to a file 
            using var wBook = new XLWorkbook();
            wBook.AddWorksheet("info");
            MemoryStream resultStream = new MemoryStream();
            wBook.SaveAs(resultStream);
            resultStream.Position = 0;

            // Copy the created file into the OneDrive using GraphApi request
            await _graphServiceClient.Me.Drive.Items[path]
                .ItemWithPath(name)
                .Content
                .Request()
                .PutAsync<DriveItem>(resultStream);
            resultStream.Close();
        }

        public async Task AddModule(AddModule command)
        {
            // Get Id of the file. The open Excel file and the corresponding sheet
            var sourceId = await GetItemId(command.SourcePath);
            var source = await OpenFile(sourceId);
            var workbook1 = new XLWorkbook(source);
            var ws1 = workbook1.Worksheet(1);
            var rows = ws1.Rows();

            // Open the database Excel file.
            var file = await OpenFile(command.dbPath);
            var workbook = new XLWorkbook(file);
            var worksheet = workbook.AddWorksheet($"{command.Name}");

            // Crate 4 tables for storing student data
            var students = new List<Student>();
            var attendance = new DataTable("attendance");
            var mark = new DataTable("mark");
            var checkpoint = new DataTable("checkpoint");
            attendance.Columns.Add(new DataColumn("Id"));
            mark.Columns.Add(new DataColumn("Id"));
            checkpoint.Columns.Add(new DataColumn("Id"));
  

            // Fill info table with the data
            foreach (var item in rows)
            {
                // Select only rows that they row number is equal or more than the starting row command
                if (item.RowNumber() >= Convert.ToInt32(command.StartingRow))
                {
                    // Retrieving Id for each row. 
                    var infoCell = item.Cell(command.IdCol);

                    // Standardize the Id (Remove extra characters)
                    string id = infoCell.GetValue<string>().Standard();
                    if (!string.IsNullOrEmpty(id))
                    {
                        // Retrieving data from each row
                        var firstName = item.Cell(command.FirstNameCol).GetValue<string>();
                        var lastName = item.Cell(command.LastNameCol).GetValue<string>();
                        var email = item.Cell(command.EmailCol).GetValue<string>();
                        // Create a new student based on the retrieved data
                        students.Add(new Student(id,email,firstName,lastName));

                        // Adding Id to all other tables
                        DataRow attendanceRow = attendance.NewRow();
                        attendanceRow["Id"] = id;
                        attendance.Rows.Add(attendanceRow);
                        DataRow markRow = mark.NewRow();
                        markRow["Id"] = id;
                        mark.Rows.Add(markRow);
                        DataRow checkpointRow = checkpoint.NewRow();
                        checkpointRow["Id"] = id;
                        checkpoint.Rows.Add(checkpointRow);

                    }
                }

            }
            // Insert tables in the sheet
            worksheet.Cell(1, 1).InsertTable(students.AsEnumerable(), "info");
            var count =worksheet.RowsUsed().Count();
            worksheet.Cell(count+2,1).InsertTable(attendance, "attendance");
            count = worksheet.RowsUsed().Count();
            worksheet.Cell(count + 2, 1).InsertTable(mark, "mark");
            count = worksheet.RowsUsed().Count();
            worksheet.Cell(count + 2, 1).InsertTable(checkpoint, "checkpoint");

            source.Close();

            // Create a steram from the Excel
            MemoryStream resultStream = new MemoryStream();
            workbook.SaveAs(resultStream);
            resultStream.Position = 0;

            // Save file into the OneDrive using Graph API
            await _graphServiceClient.Me.Drive.Items[command.dbPath].Content
                .Request()
                .PutAsync<DriveItem>(resultStream);
            resultStream.Close();
            file.Close();

        }

        public async Task<List<string>> GetModules(string path)
        {
            // Open the an Excel file and retrieve all available sheets
            var file = await OpenFile(path);
            var workbook = new XLWorkbook(file);
            return workbook.Worksheets.Select(x => x.Name).ToList();

        }

        public async Task<List<Student>> GetStudents(string path, string module)
        {
            // Initializing the list of students
            var students = new List<Student>();
            // Open Excel file and selected sheet
            var file = await OpenFile(path);
            var workbook = new XLWorkbook(file);
            var worksheet = workbook.Worksheet(module);
            // Retrieve all tables then select the required table
            var tables = worksheet.Tables;
            var table = tables.FirstOrDefault(x => x.Name.Contains("info")).AsNativeDataTable();

            // Create a new student from each row of the selected table
            foreach (DataRow row in table.Rows)
            {

                var student = new Student(row["Id"].ToString(), row["Email"].ToString(), row["Forename"].ToString(),
                    row["Surname"].ToString());
                students.Add(student);
            }
            return students;
        }

        public async Task<List<ReportItem>> GetReportItem(string path, string module)
        {
            // Open Excel file and the corresponding sheet
            var result = new List<ReportItem>();
            var file = await OpenFile(path);
            var workbook = new XLWorkbook(file);
            var worksheet = workbook.Worksheet(module);

            // Retrieve all tables
            var tables = worksheet.Tables;

            // For each table retrieve the list of header
            foreach (var table in tables)
            {
                foreach (var cel  in table.FirstRowUsed().Cells())
                {
                    result.Add(new ReportItem()
                    {
                        table = table.Name,
                        title = cel.Value.ToString()
                    });
                }

            }
            return result;
        }

        public async Task AddToDb(AddToFile command)
        {
            // Open Excel file and the corresponding sheet
            var sourceStream = await OpenFile(command.SourcePath);
            var sourceBook = new XLWorkbook(sourceStream);
            var sourceSheet = sourceBook.Worksheet(command.Module);

            // Retrieve all tables then select the required table
            var tables = sourceSheet.Tables;
            var sourceTable = tables.FirstOrDefault(x=>x.Name.Contains(command.Type)).AsNativeDataTable();
            
            // Open the file and select the first sheet
            var fileId = await GetItemId(command.FilePath);
            var fileStream = await OpenFile(fileId);
            var fileBook = new XLWorkbook(fileStream);
            var fileSheet = fileBook.Worksheet(1);

            // Get total number of rows
            var rowUsed = fileSheet.LastRowUsed().RowNumber();

            // Create a new table and add Id column to it
            var fileTable = new DataTable();
            fileTable.Columns.Add(new DataColumn("Id"));
            bool newCol = true;

            for (var row = command.StartRow + 1; row < rowUsed + 1; row++)
            {

                DataRow fileRow = fileTable.NewRow();

                // Find Id of each row from the added file then standardize it.
                var id = fileSheet.Cell(row, command.IdCol).Value.ToString().Standard();
                if (!string.IsNullOrEmpty(id))
                {
                    // Add Id value to the new table.
                    fileRow["Id"] = id;
                    foreach (var col in command.Columns)
                    {
                        // Add each value of selected columns to the new table.
                        var data = fileSheet.Cell(row, col).Value;
                        var header = fileSheet.Cell(command.StartRow, col).Value.ToString();
                        if (newCol)
                            fileTable.Columns.Add(new DataColumn(header));

                        fileRow[header] = data;


                    }
                    fileTable.Rows.Add(fileRow);

                    // Because we already added column to the table, next time we only need to update it.
                    newCol = false;
                }

            }

            // Setting primary key for both database table and new table and merge them.
            sourceTable.PrimaryKey = new DataColumn[] { sourceTable.Columns["Id"] };
            fileTable.PrimaryKey = new DataColumn[] { fileTable.Columns["Id"] };
            sourceTable.Merge(fileTable,false);

            // Find the table which we want to add data on it
            var resultTable = tables.FirstOrDefault(x => x.Name.Contains(command.Type)); 

            // Calculate column number for both new table and source table
            var resultCount = resultTable.ColumnCount();
            var sourceCount = sourceTable.Columns.Count;

            // Find the position of the new added column
            if (resultCount < sourceCount)
            {
                resultTable.InsertColumnsAfter(sourceCount - resultCount);
            }

            // Change old table with the new one
            resultTable.ReplaceData(sourceTable,false);
            var sourceColumns = sourceTable.Columns;

            // Adding new headers to the table
            var headers = new List<string>();
            foreach (DataColumn col in sourceColumns)
            {
                headers.Add(col.ColumnName);
            }
            var firstRow = resultTable.FirstRowUsed().Cells();
            var j = 0;
            foreach (var cell in firstRow)
            {
                cell.Value = headers[j];
                j++;
            }
            fileStream.Close();

            // Create a new stream and save Excel on it
            MemoryStream resultStream = new MemoryStream();
            sourceBook.SaveAs(resultStream);
            resultStream.Position = 0;

            // Send a request to Graph API and save Excel file to OneDrive
            await _graphServiceClient.Me.Drive.Items[command.SourcePath].Content
                .Request()
                .PutAsync<DriveItem>(resultStream);
            resultStream.Close();
            sourceStream.Close();

        }

        public async Task<List<StudentProgressInfo>> GetStudentProgress(string path)
        {
            var result = new List<StudentProgress>();

            // Open Excel file and the corresponding sheet
            var sourceStream = await OpenFile(path);
            var sourceBook = new XLWorkbook(sourceStream);
            // Get list of all sheet in the excel file but delete the first one
            var sheets = sourceBook.Worksheets;
            sheets.Delete("info");

            // Iterate trough each sheet and add all tables to the result
            foreach (var sheet in sheets)
            {
                
                var progress = new StudentProgress
                {
                    Module = sheet.Name,
                    Info = sheet.Tables.FirstOrDefault(x => x.Name.Contains("info")).AsNativeDataTable(),
                    Mark = sheet.Tables.FirstOrDefault(x => x.Name.Contains("mark")).AsNativeDataTable(),
                    Attendance = sheet.Tables.FirstOrDefault(x => x.Name.Contains("attendance")).AsNativeDataTable(),
                    Checkpoint = sheet.Tables.FirstOrDefault(x => x.Name.Contains("checkpoint")).AsNativeDataTable(),
                };
                result.Add(progress);
            }
            var resultInfo = new List<StudentProgressInfo>();

            // Foreach table in each sheet we will save it as a data table
            foreach (var item in result)
            {
                
                var info = item.Info;
                var mark = item.Mark;
                var attendance = item.Attendance;
                var checkpoint = item.Checkpoint;
                foreach (DataRow row in info.Rows)
                {
                    // It checks whether this Id already available in result or not
                    var id = row["Id"].ToString();
                    if (!resultInfo.Any(x=>x.Id == id))
                    {
                        // Create a new Item
                        var studentProgress = new StudentProgressInfo
                        {
                            ModuleProgress = new List<ModuleProgressInfo>(),
                            Id = id,
                            Email = row["Email"].ToString(),
                            Surname = row["Surname"].ToString(),
                            Forename = row["Forename"].ToString()
                        };

                        // Find the row for this Id in all tables
                        DataRow markRow = mark.Rows.Cast<DataRow>().FirstOrDefault(x => x["Id"] == id);
                        DataRow attendanceRow = attendance.Rows.Cast<DataRow>().FirstOrDefault(x => x["Id"] == id);
                        DataRow checkpointRow = checkpoint.Rows.Cast<DataRow>().FirstOrDefault(x => x["Id"] == id);

                        // Because we want to show module list in the first page we need a sperate item for it
                        var moduleList = new ModuleProgressInfo()
                        {
                            Name = item.Module,
                            Attendance = attendanceRow,
                            Checkpoint = checkpointRow,
                            Mark = markRow,
                        };
                        studentProgress.ModuleProgress.Add(moduleList);
                        resultInfo.Add(studentProgress);
                    }
                    else
                    {
                        // Find Id in list and add data row on it.
                        var studentProgress = resultInfo.FirstOrDefault(x => x.Id == id);
                        DataRow markRow = mark.Rows.Cast<DataRow>().FirstOrDefault(x => x["Id"] == id);
                        DataRow attendanceRow = attendance.Rows.Cast<DataRow>().FirstOrDefault(x => x["Id"] == id);
                        DataRow checkpointRow = checkpoint.Rows.Cast<DataRow>().FirstOrDefault(x => x["Id"] == id);
                        var moduleList = new ModuleProgressInfo()
                        {
                            Name = item.Module,
                            Attendance = attendanceRow,
                            Checkpoint = checkpointRow,
                            Mark = markRow,
                        };
                        studentProgress.ModuleProgress.Add(moduleList);
                    }

                }
                
            }
            return resultInfo;
        }

        public async Task<List<UpdateTable>> CheckColumns(AddToFile command)
        {
            var result = new ConfirmationModel()
            {
                NewHeaders = new List<string>()
            };
            // Open Excel file and the corresponding sheet
            var sourceStream = await OpenFile(command.SourcePath);
            var sourceBook = new XLWorkbook(sourceStream);
            var sourceSheet = sourceBook.Worksheet(command.Module);

            // Retrieve all tables then select the required table
            var tables = sourceSheet.Tables;
            var sourceTable = tables.FirstOrDefault(x => x.Name.Contains(command.Type)).AsNativeDataTable();
            
            // Set Primary key of the table
            sourceTable.PrimaryKey = new DataColumn[] { sourceTable.Columns["Id"] };
            result.OldData = sourceTable;

            // Open the file and select the first sheet
            var fileId = await GetItemId(command.FilePath);
            var fileStream = await OpenFile(fileId);
            var fileBook = new XLWorkbook(fileStream);
            var fileSheet = fileBook.Worksheet(1);

            // Get total number of rows
            var rowUsed = fileSheet.LastRowUsed().RowNumber();

            // Create a new table and add Id column to it
            var fileTable = new DataTable();
            fileTable.Columns.Add(new DataColumn("Id"));
            bool newCol = true;

            // Get headers name from the selected column numbers and add them to the result
            foreach (var head in command.Columns)
            {
                result.NewHeaders.Add(fileSheet.Cell(command.StartRow,head).Value.ToString());
            }

            for (var row = command.StartRow + 1; row < rowUsed + 1; row++)
            {
                // Find Id of each row from the added file then standardize it.
                DataRow fileRow = fileTable.NewRow();
                var id = fileSheet.Cell(row, command.IdCol).Value.ToString().Standard();
                if (!string.IsNullOrEmpty(id))
                {
                    // Add Id value to the new table.
                    fileRow["Id"] = id;
                    foreach (var col in command.Columns)
                    {
                        var data = fileSheet.Cell(row, col).Value;
                        var header = fileSheet.Cell(command.StartRow, col).Value.ToString();
                        if (newCol)
                            fileTable.Columns.Add(new DataColumn(header));

                        fileRow[header] = data;


                    }
                    fileTable.Rows.Add(fileRow);
                    // Because we already added column to the table, next time we only need to update it.

                    newCol = false;
                }

            }
            fileTable.PrimaryKey = new DataColumn[] { fileTable.Columns["Id"] };


            // Here we add a new table and add the columns which are already available in the source fill.
            // these are the old values of that columns
            var resultTable = new DataTable();
            foreach (DataColumn col in fileTable.Columns)
            {
                if (sourceTable.Columns.Contains(col.ColumnName))
                {
                    resultTable.Columns.Add(new DataColumn(col.ColumnName));
                    
                }
                
            }

            var updateTable = new List<UpdateTable>();
            // We check whether we have any columns which are available before or not
            if (resultTable.Columns.Count>1)
            {
                foreach (DataRow row in sourceTable.Rows)
                {
                    // If there is any we add the old value to the previous value with ---> delimiter
                    // with this method we can separate them later on to display to users
                    var newItem = new UpdateTable();
                    newItem.Id = row["Id"].ToString();
                    foreach (DataColumn col in resultTable.Columns)
                    {
                        if (col.ColumnName != "Id")
                        {
                            var newData = fileTable.Rows.Find(row["Id"]);
                            if (newData != null)
                            {
                                var newOne = row[col.ColumnName].ToString();
                                var oldOne = newData[col.ColumnName].ToString();
                                var newTableItem = new UpdateTableItem(oldOne, newOne,
                                    col.ColumnName);
                                if (oldOne != newOne)
                                    newItem.IsDifferent = true;
                                newItem.Items.Add(newTableItem);

                            }
                        }

                    }
                    
                    updateTable.Add(newItem);

                }



            }


            return updateTable;
        }
    }
}
