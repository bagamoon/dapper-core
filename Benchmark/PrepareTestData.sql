If (Object_Id('Bets') Is not Null)
Begin
    Drop Table Bets
End

If (Object_Id('Members') Is not Null)
Begin
    Drop Table Members
End

Create Table Members
(
	MemberId bigint identity primary key, 
	MemberCode varchar(255),	
	DateCreated datetime not null, 
	DateUpdated datetime not null
);

CREATE NONCLUSTERED INDEX [MemberCode]
    ON [dbo].[Members]([MemberCode] ASC);

Create Table Bets
(
	BetId bigint identity primary key, 
	MemberId bigint not null,
	StakeAmount decimal(26,6) not null, 
	DateCreated datetime not null, 
	DateUpdated datetime not null,
	CONSTRAINT [FK_Bets_Members] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Members] ([MemberId])
);

CREATE NONCLUSTERED INDEX [Bets]
    ON [dbo].[Bets]([MemberId] ASC);

	   
Set NoCount On;
Declare @i int = 1;
Declare @memberCount int = 20000
Declare @betCount int = 10000000

While @i <= @memberCount
Begin
	Insert Members (MemberCode, DateCreated, DateUpdated) values (CONVERT(varchar(255), NEWID()), getdate(), getdate())
	Set @i = @i + 1;
END

Set @i = 1;
While @i <= @betCount
Begin
	Insert Bets (MemberId, StakeAmount, DateCreated, DateUpdated) values (@i % @memberCount + 1, round(rand() * 1000, 2), GETDATE(), GETDATE());
	Set @i = @i + 1;
End       