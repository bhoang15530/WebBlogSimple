﻿CREATE TABLE Users ( 
	UserId INT PRIMARY KEY IDENTITY,
	Username NVARCHAR(50) NOT NULL,
	Password NVARCHAR(50) NOT NULL
);

CREATE TABLE Blogs (
	BlogId INT PRIMARY KEY IDENTITY,
	Title NVARCHAR(100) NOT NULL,
	Content NVARCHAR(MAX) NOT NULL,
	UserId INT NOT NULL,
	FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
