# ImageSlayer
- This is a console application could have included in the main project but it could be heave enough to run slow.
- After the successful mechanism, the code would be added as one of the module of the main project-Sifaris.
  
### Requirements before running this project
i.     Database which has table called ScannedDocument with the below respective field 
       ScannedDocumentId	nvarchar(50)	- PK
       ScannedFile	varchar(MAX) - base64 image text
       RegistrationId	nvarchar(50) - null
       RegistrationInvoiceId	nvarchar(50)	- FK from RegistrationInvoiceTable	
       ScannedFileName	varchar(MAX) - name of the file e.g. धरजग्गा.pdf,धरजग्गा.jpeg,धरजग्गा.png, धरजग्गा.htm etc 

ii. Have some data in the above table and run this very console application to change those text into images and save those images in the given path with file name mentioned in the code on the otherhand update the  **ScannedFile** and **ScannedFileName** synchronously.


		
