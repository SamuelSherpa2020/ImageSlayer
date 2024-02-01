# ImageSlayer -Intro
- This is a C# console application built to convert base64 text saved in a database back into it's original existance which could be image or file.
- Basically the limited space for a DB in SSMS which was 10GB got filled for one of my active project. So this ImageSlayer was built to convert almost 10 GB of base64 text into image and save it in wwwroot\Files\ leaving DB empty to few MBs.
- In the project you could see how wisely  ROW_NUMBER() was used to count the total rows, then just convert the base64 texts in the batch of 100 to avoid sql connection timeout or cannot read data in the datatable in the console app.
- After publish, the exe file was run in the server which gave positive results.


		
