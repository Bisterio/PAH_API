USE [PlatformAntiquesHandicrafts]
GO
/****** Object:  Table [dbo].[ActivationRequest]    Script Date: 09/11/2023 13:32:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ActivationRequest](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[email] [varchar](255) NOT NULL,
	[timestamp] [datetime] NULL,
	[content] [nvarchar](500) NULL,
	[status] [int] NULL,
	[staffId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Address]    Script Date: 09/11/2023 13:32:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Address](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[customerId] [int] NULL,
	[recipientName] [nvarchar](255) NULL,
	[recipientPhone] [nvarchar](255) NULL,
	[province] [nvarchar](255) NULL,
	[provinceId] [int] NULL,
	[district] [nvarchar](255) NULL,
	[districtId] [int] NULL,
	[ward] [nvarchar](255) NULL,
	[wardCode] [varchar](10) NULL,
	[street] [nvarchar](255) NULL,
	[type] [int] NOT NULL,
	[isDefault] [bit] NULL,
	[createdAt] [datetime] NULL,
	[updatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Auction]    Script Date: 09/11/2023 13:32:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Auction](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[productId] [int] NULL,
	[staffId] [int] NULL,
	[title] [nvarchar](255) NOT NULL,
	[entryFee] [decimal](12, 1) NOT NULL,
	[startingPrice] [decimal](12, 1) NOT NULL,
	[step] [decimal](12, 1) NOT NULL,
	[status] [int] NOT NULL,
	[startedAt] [datetime] NULL,
	[endedAt] [datetime] NULL,
	[maxEndedAt] [datetime] NULL,
	[createdAt] [datetime] NULL,
	[updatedAt] [datetime] NULL,
	[registrationStart] [datetime] NULL,
	[registrationEnd] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Bid]    Script Date: 09/11/2023 13:32:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bid](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[auctionId] [int] NULL,
	[bidderId] [int] NULL,
	[bidAmount] [decimal](12, 1) NULL,
	[bidDate] [datetime] NULL,
	[status] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Buyer]    Script Date: 09/11/2023 13:32:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Buyer](
	[id] [int] NOT NULL,
	[joinedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Category]    Script Date: 09/11/2023 13:32:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Category](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](255) NOT NULL,
	[imageUrl] [nvarchar](max) NULL,
	[createdAt] [datetime] NULL,
	[updatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Feedback]    Script Date: 09/11/2023 13:32:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Feedback](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[productId] [int] NULL,
	[buyerId] [int] NULL,
	[ratings] [float] NULL,
	[buyerFeedback] [nvarchar](max) NULL,
	[status] [int] NOT NULL,
	[timestamp] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Material]    Script Date: 09/11/2023 13:32:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Material](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](255) NOT NULL,
	[createdAt] [datetime] NULL,
	[updatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Order]    Script Date: 09/11/2023 13:32:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Order](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[buyerId] [int] NULL,
	[sellerId] [int] NULL,
	[recipientName] [nvarchar](255) NULL,
	[recipientPhone] [char](10) NULL,
	[recipientAddress] [nvarchar](max) NULL,
	[orderDate] [datetime] NULL,
	[totalAmount] [decimal](12, 1) NULL,
	[orderShippingCode] [nvarchar](20) NULL,
	[shippingCost] [decimal](12, 1) NULL,
	[status] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderCancellation]    Script Date: 09/11/2023 13:32:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderCancellation](
	[id] [int] NOT NULL,
	[reason] [nvarchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderItem]    Script Date: 09/11/2023 13:32:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderItem](
	[orderId] [int] NOT NULL,
	[productId] [int] NOT NULL,
	[price] [decimal](12, 1) NULL,
	[quantity] [int] NULL,
	[imageUrl] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[orderId] ASC,
	[productId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Product]    Script Date: 09/11/2023 13:32:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Product](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[categoryId] [int] NULL,
	[materialId] [int] NULL,
	[sellerId] [int] NULL,
	[name] [nvarchar](255) NOT NULL,
	[description] [nvarchar](max) NULL,
	[price] [decimal](12, 1) NOT NULL,
	[dimension] [nvarchar](255) NULL,
	[weight] [decimal](6, 1) NULL,
	[origin] [nvarchar](255) NULL,
	[packageMethod] [nvarchar](255) NULL,
	[packageContent] [nvarchar](255) NULL,
	[condition] [int] NOT NULL,
	[type] [int] NOT NULL,
	[status] [int] NOT NULL,
	[ratings] [decimal](2, 1) NULL,
	[createdAt] [datetime] NULL,
	[updatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProductImage]    Script Date: 09/11/2023 13:32:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductImage](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[productId] [int] NULL,
	[status] [int] NOT NULL,
	[imageUrl] [nvarchar](max) NULL,
	[createdAt] [datetime] NULL,
	[updatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Response]    Script Date: 09/11/2023 13:32:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Response](
	[feedbackId] [int] NOT NULL,
	[sellerId] [int] NULL,
	[sellerMessage] [nvarchar](max) NULL,
	[timestamp] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[feedbackId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Seller]    Script Date: 09/11/2023 13:32:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Seller](
	[id] [int] NOT NULL,
	[name] [nvarchar](255) NOT NULL,
	[phone] [char](10) NOT NULL,
	[profilePicture] [varchar](max) NULL,
	[registeredAt] [datetime] NULL,
	[ratings] [decimal](2, 1) NULL,
	[status] [int] NOT NULL,
	[shopId] [varchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Token]    Script Date: 09/11/2023 13:32:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Token](
	[id] [int] NOT NULL,
	[refreshToken] [varchar](512) NULL,
	[expiryTime] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Transaction]    Script Date: 09/11/2023 13:32:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Transaction](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[walletId] [int] NULL,
	[paymentMethod] [int] NULL,
	[amount] [decimal](12, 1) NULL,
	[type] [int] NOT NULL,
	[date] [datetime] NULL,
	[description] [nvarchar](255) NULL,
	[status] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[User]    Script Date: 09/11/2023 13:32:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](255) NOT NULL,
	[email] [varchar](255) NOT NULL,
	[password] [varchar](60) NOT NULL,
	[phone] [char](10) NULL,
	[profilePicture] [varchar](max) NULL,
	[gender] [int] NULL,
	[dob] [datetime] NULL,
	[role] [int] NOT NULL,
	[status] [int] NOT NULL,
	[createdAt] [datetime] NULL,
	[updatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VerifyToken]    Script Date: 09/11/2023 13:32:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VerifyToken](
	[id] [int] NOT NULL,
	[code] [varchar](512) NOT NULL,
	[expirationDate] [datetime] NOT NULL,
	[status] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Wallet]    Script Date: 09/11/2023 13:32:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Wallet](
	[id] [int] NOT NULL,
	[availableBalance] [decimal](12, 1) NULL,
	[lockedBalance] [decimal](12, 1) NULL,
	[status] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Withdrawal]    Script Date: 09/11/2023 13:32:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Withdrawal](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[WalletId] [int] NOT NULL,
	[ManagerId] [int] NULL,
	[Amount] [decimal](12, 1) NOT NULL,
	[Bank] [varchar](20) NOT NULL,
	[BankNumber] [varchar](20) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NOT NULL,
	[Status] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Address] ON 

INSERT [dbo].[Address] ([id], [customerId], [recipientName], [recipientPhone], [province], [provinceId], [district], [districtId], [ward], [wardCode], [street], [type], [isDefault], [createdAt], [updatedAt]) VALUES (1, 7, N'asd', N'0901234567', N'Hồ Chí Minh', 202, N'Thành Phố Thủ Đức', 3695, N'Phường Bình Chiểu', N'90736', N'80/40a, Ngô Chí Quốc', 1, 1, CAST(N'2023-10-12T00:00:00.000' AS DateTime), CAST(N'2023-10-12T00:00:00.000' AS DateTime))
INSERT [dbo].[Address] ([id], [customerId], [recipientName], [recipientPhone], [province], [provinceId], [district], [districtId], [ward], [wardCode], [street], [type], [isDefault], [createdAt], [updatedAt]) VALUES (2, 6, N'3rodeo', N'0907654321', N'Hồ Chí Minh', 202, N'Thành Phố Thủ Đức', 3695, N'Phường Bình Chiểu', N'90736', N'989, Tinh Lo 43', 2, 1, CAST(N'2023-11-01T00:00:00.000' AS DateTime), CAST(N'2023-11-01T00:00:00.000' AS DateTime))
INSERT [dbo].[Address] ([id], [customerId], [recipientName], [recipientPhone], [province], [provinceId], [district], [districtId], [ward], [wardCode], [street], [type], [isDefault], [createdAt], [updatedAt]) VALUES (3, 13, N'asd', N'0901762534', N'Hồ Chí Minh', 202, N'Thành Phố Thủ Đức', 3695, N'Phường Bình Chiểu', N'90736', N'80/40a, Ngô Chí Quốc', 1, 1, CAST(N'2023-10-12T00:00:00.000' AS DateTime), CAST(N'2023-10-12T00:00:00.000' AS DateTime))
SET IDENTITY_INSERT [dbo].[Address] OFF
GO
SET IDENTITY_INSERT [dbo].[Auction] ON 

INSERT [dbo].[Auction] ([id], [productId], [staffId], [title], [entryFee], [startingPrice], [step], [status], [startedAt], [endedAt], [createdAt], [updatedAt], [registrationStart], [registrationEnd]) VALUES (1, 11, 6, N'Test', CAST(0.0 AS Decimal(12, 1)), CAST(0.0 AS Decimal(12, 1)), CAST(0.0 AS Decimal(12, 1)), 6, CAST(N'2023-10-25T00:00:00.000' AS DateTime), CAST(N'2023-10-25T00:00:00.000' AS DateTime), CAST(N'2023-10-25T00:00:00.000' AS DateTime), CAST(N'2023-10-25T00:00:00.000' AS DateTime), CAST(N'2023-10-25T00:00:00.000' AS DateTime), CAST(N'2023-10-25T00:00:00.000' AS DateTime))
SET IDENTITY_INSERT [dbo].[Auction] OFF
GO
SET IDENTITY_INSERT [dbo].[Bid] ON 

INSERT [dbo].[Bid] ([id], [auctionId], [bidderId], [bidAmount], [bidDate], [status]) VALUES (2, 1, 7, CAST(10000.0 AS Decimal(12, 1)), CAST(N'2023-10-25T00:00:00.000' AS DateTime), 1)
SET IDENTITY_INSERT [dbo].[Bid] OFF
GO
INSERT [dbo].[Buyer] ([id], [joinedAt]) VALUES (1, CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Buyer] ([id], [joinedAt]) VALUES (2, CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Buyer] ([id], [joinedAt]) VALUES (3, CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Buyer] ([id], [joinedAt]) VALUES (4, CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Buyer] ([id], [joinedAt]) VALUES (5, CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Buyer] ([id], [joinedAt]) VALUES (6, CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Buyer] ([id], [joinedAt]) VALUES (7, CAST(N'2023-10-10T19:12:18.397' AS DateTime))
INSERT [dbo].[Buyer] ([id], [joinedAt]) VALUES (12, CAST(N'2023-11-01T15:14:25.660' AS DateTime))
INSERT [dbo].[Buyer] ([id], [joinedAt]) VALUES (13, CAST(N'2023-11-01T14:32:28.687' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[Category] ON 

INSERT [dbo].[Category] ([id], [name], [imageUrl], [createdAt], [updatedAt]) VALUES (1, N'Đá phong thuỷ', N'https://media.loveitopcdn.com/26429/800k-2.jpg', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Category] ([id], [name], [imageUrl], [createdAt], [updatedAt]) VALUES (2, N'Trang sức cổ', N'https://vanchuyenhangquangchau.vn/file/tuvan/1634630797-trang-suc-co-trang-trung-quoc.jpg', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Category] ([id], [name], [imageUrl], [createdAt], [updatedAt]) VALUES (3, N'Nội thất cổ', N'https://antiquesworld.co.uk/wp-content/uploads/2021/04/antique-furniture.jpg', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Category] ([id], [name], [imageUrl], [createdAt], [updatedAt]) VALUES (4, N'Trang sức phong thuỷ', N'https://vcdn-giadinh.vnecdn.net/2020/05/01/5d0c91a77fcc7-NQVT0745-V-ng-Ta-5030-3611-1588267934.jpg', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
SET IDENTITY_INSERT [dbo].[Category] OFF
GO
SET IDENTITY_INSERT [dbo].[Feedback] ON 

INSERT [dbo].[Feedback] ([id], [productId], [buyerId], [ratings], [buyerFeedback], [status], [timestamp]) VALUES (1, 1, 1, 5, N'Sản phẩm rất tuyệt, đá rất đẹp', 1, CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Feedback] ([id], [productId], [buyerId], [ratings], [buyerFeedback], [status], [timestamp]) VALUES (2, 4, 3, 4.5, N'Sản phẩm tuyệt', 1, CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Feedback] ([id], [productId], [buyerId], [ratings], [buyerFeedback], [status], [timestamp]) VALUES (3, 8, 5, 5, N'Không có gì để chê', 1, CAST(N'2023-09-27T19:55:51.333' AS DateTime))
SET IDENTITY_INSERT [dbo].[Feedback] OFF
GO
SET IDENTITY_INSERT [dbo].[Material] ON 

INSERT [dbo].[Material] ([id], [name], [createdAt], [updatedAt]) VALUES (1, N'Đá quý', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Material] ([id], [name], [createdAt], [updatedAt]) VALUES (2, N'Gỗ', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Material] ([id], [name], [createdAt], [updatedAt]) VALUES (3, N'Bạc', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Material] ([id], [name], [createdAt], [updatedAt]) VALUES (4, N'Thuỷ tinh', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Material] ([id], [name], [createdAt], [updatedAt]) VALUES (5, N'Đồng', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Material] ([id], [name], [createdAt], [updatedAt]) VALUES (6, N'Sắt', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Material] ([id], [name], [createdAt], [updatedAt]) VALUES (7, N'Titan', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Material] ([id], [name], [createdAt], [updatedAt]) VALUES (8, N'Vải', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Material] ([id], [name], [createdAt], [updatedAt]) VALUES (9, N'Lụa', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
SET IDENTITY_INSERT [dbo].[Material] OFF
GO
SET IDENTITY_INSERT [dbo].[Order] ON 

INSERT [dbo].[Order] ([id], [buyerId], [sellerId], [recipientName], [recipientPhone], [recipientAddress], [orderDate], [totalAmount], [orderShippingCode], [shippingCost], [status]) VALUES (1, 1, 2, N'Vũ Triều Dương', N'0909000001', N'20 Man Thiện', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(1220000.0 AS Decimal(12, 1)), NULL, CAST(30000.0 AS Decimal(12, 1)), 1)
INSERT [dbo].[Order] ([id], [buyerId], [sellerId], [recipientName], [recipientPhone], [recipientAddress], [orderDate], [totalAmount], [orderShippingCode], [shippingCost], [status]) VALUES (2, 3, 2, N'Trần Ngọc Châu', N'0909000003', N'185 Gò Dầu', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(1216000.0 AS Decimal(12, 1)), NULL, CAST(25000.0 AS Decimal(12, 1)), 1)
INSERT [dbo].[Order] ([id], [buyerId], [sellerId], [recipientName], [recipientPhone], [recipientAddress], [orderDate], [totalAmount], [orderShippingCode], [shippingCost], [status]) VALUES (3, 5, 4, N'Lê Minh Đức', N'090900005 ', N'18 Hoàng Hữu Nam', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(20000000.0 AS Decimal(12, 1)), NULL, CAST(30000.0 AS Decimal(12, 1)), 1)
INSERT [dbo].[Order] ([id], [buyerId], [sellerId], [recipientName], [recipientPhone], [recipientAddress], [orderDate], [totalAmount], [orderShippingCode], [shippingCost], [status]) VALUES (13, 7, 2, N'asd', N'123456789 ', N'asd str qwe, c, b, a', CAST(N'2023-10-14T13:21:54.400' AS DateTime), CAST(7650000.0 AS Decimal(12, 1)), NULL, CAST(8000.0 AS Decimal(12, 1)), 1)
INSERT [dbo].[Order] ([id], [buyerId], [sellerId], [recipientName], [recipientPhone], [recipientAddress], [orderDate], [totalAmount], [orderShippingCode], [shippingCost], [status]) VALUES (14, 7, 6, N'asd', N'123456789 ', N'asd str qwe, c, b, a', CAST(N'2023-10-14T13:21:54.400' AS DateTime), CAST(1550000.0 AS Decimal(12, 1)), NULL, CAST(8000.0 AS Decimal(12, 1)), 1)
INSERT [dbo].[Order] ([id], [buyerId], [sellerId], [recipientName], [recipientPhone], [recipientAddress], [orderDate], [totalAmount], [orderShippingCode], [shippingCost], [status]) VALUES (15, 7, 2, N'asd', N'123456789 ', N'asd str qwe, c, b, a', CAST(N'2023-10-14T13:23:49.340' AS DateTime), CAST(7650000.0 AS Decimal(12, 1)), NULL, CAST(8000.0 AS Decimal(12, 1)), 1)
INSERT [dbo].[Order] ([id], [buyerId], [sellerId], [recipientName], [recipientPhone], [recipientAddress], [orderDate], [totalAmount], [orderShippingCode], [shippingCost], [status]) VALUES (16, 7, 6, N'asd', N'123456789 ', N'asd str qwe, c, b, a', CAST(N'2023-10-14T13:23:49.340' AS DateTime), CAST(1550000.0 AS Decimal(12, 1)), NULL, CAST(8000.0 AS Decimal(12, 1)), 1)
INSERT [dbo].[Order] ([id], [buyerId], [sellerId], [recipientName], [recipientPhone], [recipientAddress], [orderDate], [totalAmount], [orderShippingCode], [shippingCost], [status]) VALUES (17, 7, 2, N'asd', N'123456789 ', N'asd str qwe, c, b, a', CAST(N'2023-10-14T13:38:24.460' AS DateTime), CAST(7650000.0 AS Decimal(12, 1)), NULL, CAST(8000.0 AS Decimal(12, 1)), 5)
INSERT [dbo].[Order] ([id], [buyerId], [sellerId], [recipientName], [recipientPhone], [recipientAddress], [orderDate], [totalAmount], [orderShippingCode], [shippingCost], [status]) VALUES (18, 7, 6, N'asd', N'123456789 ', N'asd str qwe, c, b, a', CAST(N'2023-10-14T13:38:24.460' AS DateTime), CAST(1550000.0 AS Decimal(12, 1)), N'G8GKNQVG', CAST(8000.0 AS Decimal(12, 1)), 12)
INSERT [dbo].[Order] ([id], [buyerId], [sellerId], [recipientName], [recipientPhone], [recipientAddress], [orderDate], [totalAmount], [orderShippingCode], [shippingCost], [status]) VALUES (19, 7, 6, N'asd', N'123456789 ', N'asd str qwe, c, b, a', CAST(N'2023-10-25T14:27:42.473' AS DateTime), CAST(0.0 AS Decimal(12, 1)), NULL, CAST(10000.0 AS Decimal(12, 1)), 1)
INSERT [dbo].[Order] ([id], [buyerId], [sellerId], [recipientName], [recipientPhone], [recipientAddress], [orderDate], [totalAmount], [orderShippingCode], [shippingCost], [status]) VALUES (20, 7, 6, N'asd', N'123456789 ', N'asd str qwe, c, b, a', CAST(N'2023-10-25T14:28:57.250' AS DateTime), CAST(0.0 AS Decimal(12, 1)), NULL, CAST(10000.0 AS Decimal(12, 1)), 2)
INSERT [dbo].[Order] ([id], [buyerId], [sellerId], [recipientName], [recipientPhone], [recipientAddress], [orderDate], [totalAmount], [orderShippingCode], [shippingCost], [status]) VALUES (21, 13, 6, N'asd', N'123456789 ', N'80/40a, Ngô Chí Quốc, Phường Bình Chiểu, Thành Phố Thủ Đức, Hồ Chí Minh', CAST(N'2023-11-01T19:44:33.943' AS DateTime), CAST(24550000.0 AS Decimal(12, 1)), N'LFLA34', CAST(10000.0 AS Decimal(12, 1)), 6)
SET IDENTITY_INSERT [dbo].[Order] OFF
GO
INSERT [dbo].[OrderCancellation] ([id], [reason]) VALUES (18, N'Test cancel')
GO
INSERT [dbo].[OrderItem] ([orderId], [productId], [price], [quantity], [imageUrl]) VALUES (1, 1, CAST(1220000.0 AS Decimal(12, 1)), 1, N'https://media.loveitopcdn.com/25808/thumb/da-canh-thach-anh-hong-m277415-3.jpg')
INSERT [dbo].[OrderItem] ([orderId], [productId], [price], [quantity], [imageUrl]) VALUES (2, 4, CAST(1216000.0 AS Decimal(12, 1)), 1, N'https://media.loveitopcdn.com/25808/thumb/da-canh-thach-anh-hong-m277415-3.jpg')
INSERT [dbo].[OrderItem] ([orderId], [productId], [price], [quantity], [imageUrl]) VALUES (3, 8, CAST(20000000.0 AS Decimal(12, 1)), 1, N'https://media.loveitopcdn.com/25808/thumb/da-canh-thach-anh-hong-m277415-3.jpg')
INSERT [dbo].[OrderItem] ([orderId], [productId], [price], [quantity], [imageUrl]) VALUES (13, 1, CAST(1220000.0 AS Decimal(12, 1)), 1, N'https://media.loveitopcdn.com/25808/thumb/da-canh-thach-anh-hong-m277415-3.jpg')
INSERT [dbo].[OrderItem] ([orderId], [productId], [price], [quantity], [imageUrl]) VALUES (13, 2, CAST(6430000.0 AS Decimal(12, 1)), 1, N'https://media.loveitopcdn.com/25808/thumb/da-canh-fluorite-xanh-m282420.jpg')
INSERT [dbo].[OrderItem] ([orderId], [productId], [price], [quantity], [imageUrl]) VALUES (14, 6, CAST(1550000.0 AS Decimal(12, 1)), 1, N'https://cloud.muaban.net/images/2022/07/06/047/aa389f6f32ab4738bfd68313b5c52c42.jpg')
INSERT [dbo].[OrderItem] ([orderId], [productId], [price], [quantity], [imageUrl]) VALUES (15, 1, CAST(1220000.0 AS Decimal(12, 1)), 1, N'https://media.loveitopcdn.com/25808/thumb/da-canh-thach-anh-hong-m277415-3.jpg')
INSERT [dbo].[OrderItem] ([orderId], [productId], [price], [quantity], [imageUrl]) VALUES (15, 2, CAST(6430000.0 AS Decimal(12, 1)), 1, N'https://media.loveitopcdn.com/25808/thumb/da-canh-fluorite-xanh-m282420.jpg')
INSERT [dbo].[OrderItem] ([orderId], [productId], [price], [quantity], [imageUrl]) VALUES (16, 6, CAST(1550000.0 AS Decimal(12, 1)), 1, N'https://cloud.muaban.net/images/2022/07/06/047/aa389f6f32ab4738bfd68313b5c52c42.jpg')
INSERT [dbo].[OrderItem] ([orderId], [productId], [price], [quantity], [imageUrl]) VALUES (17, 1, CAST(1220000.0 AS Decimal(12, 1)), 1, N'https://media.loveitopcdn.com/25808/thumb/da-canh-thach-anh-hong-m277415-3.jpg')
INSERT [dbo].[OrderItem] ([orderId], [productId], [price], [quantity], [imageUrl]) VALUES (17, 2, CAST(6430000.0 AS Decimal(12, 1)), 1, N'https://media.loveitopcdn.com/25808/thumb/da-canh-fluorite-xanh-m282420.jpg')
INSERT [dbo].[OrderItem] ([orderId], [productId], [price], [quantity], [imageUrl]) VALUES (18, 6, CAST(1550000.0 AS Decimal(12, 1)), 1, N'https://cloud.muaban.net/images/2022/07/06/047/aa389f6f32ab4738bfd68313b5c52c42.jpg')
INSERT [dbo].[OrderItem] ([orderId], [productId], [price], [quantity], [imageUrl]) VALUES (19, 11, CAST(0.0 AS Decimal(12, 1)), 1, NULL)
INSERT [dbo].[OrderItem] ([orderId], [productId], [price], [quantity], [imageUrl]) VALUES (20, 11, CAST(0.0 AS Decimal(12, 1)), 1, NULL)
INSERT [dbo].[OrderItem] ([orderId], [productId], [price], [quantity], [imageUrl]) VALUES (21, 6, CAST(1550000.0 AS Decimal(12, 1)), 1, N'https://cloud.muaban.net/images/2022/07/06/047/aa389f6f32ab4738bfd68313b5c52c42.jpg')
INSERT [dbo].[OrderItem] ([orderId], [productId], [price], [quantity], [imageUrl]) VALUES (21, 11, CAST(23000000.0 AS Decimal(12, 1)), 1, N'https://cloud.muaban.net/images/2022/07/03/241/d41f534958be4614b385437780ef8491.jpghttps://cloud.muaban.net/images/2022/07/03/241/d41f534958be4614b385437780ef8491.jpg')
GO
SET IDENTITY_INSERT [dbo].[Product] ON 

INSERT [dbo].[Product] ([id], [categoryId], [materialId], [sellerId], [name], [description], [price], [dimension], [weight], [origin], [packageMethod], [packageContent], [condition], [type], [status], [ratings], [createdAt], [updatedAt]) VALUES (1, 1, 1, 2, N'Đá thạch anh hồng phong thuỷ', N'Đá thạch anh hồng phong thuỷ', CAST(1220000.0 AS Decimal(12, 1)), N'14.8x12x6.3', CAST(1500.0 AS Decimal(6, 1)), N'Việt Nam', N'Hộp kèm đế', N'Đá cảnh + đế gỗ + túi giấy sang trọng + dầu dưỡng đá + giấy kiểm định chất lượng đá', 0, 0, 1, CAST(0.0 AS Decimal(2, 1)), CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Product] ([id], [categoryId], [materialId], [sellerId], [name], [description], [price], [dimension], [weight], [origin], [packageMethod], [packageContent], [condition], [type], [status], [ratings], [createdAt], [updatedAt]) VALUES (2, 1, 1, 2, N'Đá thạch anh xanh phong thuỷ', N'Đá thạch anh xanh phong thuỷ', CAST(6430000.0 AS Decimal(12, 1)), N'33x22x11.6', CAST(12260.0 AS Decimal(6, 1)), N'Việt Nam', N'Hộp kèm đế', N'Đá cảnh + đế gỗ + dầu dưỡng đá + giấy kiểm định chất lượng đá', 0, 0, 1, CAST(0.0 AS Decimal(2, 1)), CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Product] ([id], [categoryId], [materialId], [sellerId], [name], [description], [price], [dimension], [weight], [origin], [packageMethod], [packageContent], [condition], [type], [status], [ratings], [createdAt], [updatedAt]) VALUES (3, 1, 1, 2, N'Đá thạch anh trắng phong thuỷ', N'Đá thạch anh trắng phong thuỷ', CAST(1960000.0 AS Decimal(12, 1)), N'27.5x17x9.5', CAST(8300.0 AS Decimal(6, 1)), N'Việt Nam', N'Hộp kèm đế', N'Đá cảnh + đế gỗ + dầu dưỡng đá + giấy kiểm định chất lượng đá', 0, 0, 1, CAST(0.0 AS Decimal(2, 1)), CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Product] ([id], [categoryId], [materialId], [sellerId], [name], [description], [price], [dimension], [weight], [origin], [packageMethod], [packageContent], [condition], [type], [status], [ratings], [createdAt], [updatedAt]) VALUES (4, 1, 1, 2, N'Đá fluorite xanh phong thuỷ', N'Đá fluorite xanh phong thuỷ', CAST(1216000.0 AS Decimal(12, 1)), N'13.8x4.5x4.3', CAST(590.0 AS Decimal(6, 1)), N'Việt Nam', N'Hộp kèm đế', N'Đá cảnh + đế gỗ + dầu dưỡng đá + giấy kiểm định chất lượng đá', 0, 0, 1, CAST(0.0 AS Decimal(2, 1)), CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Product] ([id], [categoryId], [materialId], [sellerId], [name], [description], [price], [dimension], [weight], [origin], [packageMethod], [packageContent], [condition], [type], [status], [ratings], [createdAt], [updatedAt]) VALUES (5, 1, 1, 2, N'Đá thạch anh vàng phong thuỷ', N'Đá thạch anh vàng phong thuỷ', CAST(4632000.0 AS Decimal(12, 1)), N'20.5x10x10', CAST(1190.0 AS Decimal(6, 1)), N'Việt Nam', N'Hộp kèm đế', N'Đá cảnh + đế gỗ + dầu dưỡng đá + giấy kiểm định chất lượng đá', 0, 1, 1, CAST(0.0 AS Decimal(2, 1)), CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Product] ([id], [categoryId], [materialId], [sellerId], [name], [description], [price], [dimension], [weight], [origin], [packageMethod], [packageContent], [condition], [type], [status], [ratings], [createdAt], [updatedAt]) VALUES (6, 3, 5, 6, N'Đe đồng của thợ bạc mini', N'Đe đồng của thợ bạc mini', CAST(1550000.0 AS Decimal(12, 1)), N'10x5x5', CAST(1200.0 AS Decimal(6, 1)), N'Việt Nam', N'Hộp', N'Đe đồng', 4, 1, 1, CAST(0.0 AS Decimal(2, 1)), CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Product] ([id], [categoryId], [materialId], [sellerId], [name], [description], [price], [dimension], [weight], [origin], [packageMethod], [packageContent], [condition], [type], [status], [ratings], [createdAt], [updatedAt]) VALUES (7, 4, 1, 2, N'Vòng đá mắt hổ xanh phong thuỷ', N'Vòng đá mắt hổ xanh phong thuỷ', CAST(455000.0 AS Decimal(12, 1)), N'12', CAST(100.0 AS Decimal(6, 1)), N'Việt Nam', N'Hộp', N'Vòng đá', 0, 0, 1, CAST(0.0 AS Decimal(2, 1)), CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Product] ([id], [categoryId], [materialId], [sellerId], [name], [description], [price], [dimension], [weight], [origin], [packageMethod], [packageContent], [condition], [type], [status], [ratings], [createdAt], [updatedAt]) VALUES (8, 2, 5, 4, N'Dây chuyền nanh heo rừng bằng đồng', N'Dây chuyền nanh heo rừng bằng đồng', CAST(20000000.0 AS Decimal(12, 1)), N'30', CAST(200.0 AS Decimal(6, 1)), N'Việt Nam', N'Hộp', N'Dây chuyền + nanh heo rừng ', 2, 0, 1, CAST(0.0 AS Decimal(2, 1)), CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Product] ([id], [categoryId], [materialId], [sellerId], [name], [description], [price], [dimension], [weight], [origin], [packageMethod], [packageContent], [condition], [type], [status], [ratings], [createdAt], [updatedAt]) VALUES (9, 3, 5, 4, N'Bàn ủi con gà bằng đồng', N'Bàn ủi con gà bằng đồng', CAST(9000000.0 AS Decimal(12, 1)), N'20x10x15', CAST(800.0 AS Decimal(6, 1)), N'Việt Nam', N'Hộp', N'Bàn ủi', 3, 0, 1, CAST(0.0 AS Decimal(2, 1)), CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Product] ([id], [categoryId], [materialId], [sellerId], [name], [description], [price], [dimension], [weight], [origin], [packageMethod], [packageContent], [condition], [type], [status], [ratings], [createdAt], [updatedAt]) VALUES (10, 3, 3, 4, N'Thỏi bạc quý', N'Thỏi bạc quý', CAST(25000000.0 AS Decimal(12, 1)), N'5x7x5', CAST(700.0 AS Decimal(6, 1)), N'Việt Nam', N'Hộp', N'Thỏi bạc', 2, 0, 1, CAST(0.0 AS Decimal(2, 1)), CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Product] ([id], [categoryId], [materialId], [sellerId], [name], [description], [price], [dimension], [weight], [origin], [packageMethod], [packageContent], [condition], [type], [status], [ratings], [createdAt], [updatedAt]) VALUES (11, 3, 3, 6, N'Đồng hồ để bàn Atmos 528 Jaeger Lecoultre Thụy Sỹ', N'Đồng hồ để bàn Atmos 528 Jaeger Lecoultre Thụy Sỹ', CAST(23000000.0 AS Decimal(12, 1)), N'30x20x20', CAST(1200.0 AS Decimal(6, 1)), N'Thuỵ Sĩ', N'Hộp', N'Đồng hồ bạc mạ vàng', 1, 2, 1, CAST(0.0 AS Decimal(2, 1)), CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
SET IDENTITY_INSERT [dbo].[Product] OFF
GO
SET IDENTITY_INSERT [dbo].[ProductImage] ON 

INSERT [dbo].[ProductImage] ([id], [productId], [status], [imageUrl], [createdAt], [updatedAt]) VALUES (1, 1, 1, N'https://media.loveitopcdn.com/25808/thumb/da-canh-thach-anh-hong-m277415-3.jpg', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[ProductImage] ([id], [productId], [status], [imageUrl], [createdAt], [updatedAt]) VALUES (2, 2, 1, N'https://media.loveitopcdn.com/25808/thumb/da-canh-fluorite-xanh-m282420.jpg', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[ProductImage] ([id], [productId], [status], [imageUrl], [createdAt], [updatedAt]) VALUES (3, 3, 1, N'https://media.loveitopcdn.com/25808/thumb/da-canh-thach-anh-trang-m150083-1.jpg', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[ProductImage] ([id], [productId], [status], [imageUrl], [createdAt], [updatedAt]) VALUES (4, 4, 1, N'https://media.loveitopcdn.com/25808/thumb/tru-da-fluorite-xanh-m0752059-3.jpg', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[ProductImage] ([id], [productId], [status], [imageUrl], [createdAt], [updatedAt]) VALUES (5, 5, 1, N'https://media.loveitopcdn.com/25808/thumb/img01082-copy.jpg', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[ProductImage] ([id], [productId], [status], [imageUrl], [createdAt], [updatedAt]) VALUES (6, 6, 1, N'https://cloud.muaban.net/images/2022/07/06/047/aa389f6f32ab4738bfd68313b5c52c42.jpg', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[ProductImage] ([id], [productId], [status], [imageUrl], [createdAt], [updatedAt]) VALUES (7, 7, 1, N'https://media.loveitopcdn.com/25808/thumb/img09357-copy.jpg', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[ProductImage] ([id], [productId], [status], [imageUrl], [createdAt], [updatedAt]) VALUES (8, 8, 1, N'https://cloud.muaban.net/images/2023/06/22/334/50c23df095054b36a8c6cd4176c79f00.jpg', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[ProductImage] ([id], [productId], [status], [imageUrl], [createdAt], [updatedAt]) VALUES (9, 9, 1, N'https://cloud.muaban.net/images/thumb-detail/2022/05/11/497/375f25ac384c4de6805b153996d77d2d.jpg', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[ProductImage] ([id], [productId], [status], [imageUrl], [createdAt], [updatedAt]) VALUES (10, 10, 1, N'https://cloud.muaban.net/images/2023/07/29/586/1f7e228a67674cdf9e3a2d07632475fe.jpg', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[ProductImage] ([id], [productId], [status], [imageUrl], [createdAt], [updatedAt]) VALUES (11, 11, 1, N'https://cloud.muaban.net/images/2022/07/03/241/d41f534958be4614b385437780ef8491.jpghttps://cloud.muaban.net/images/2022/07/03/241/d41f534958be4614b385437780ef8491.jpg', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
SET IDENTITY_INSERT [dbo].[ProductImage] OFF
GO
INSERT [dbo].[Response] ([feedbackId], [sellerId], [sellerMessage], [timestamp]) VALUES (1, 2, N'Xin cảm ơn quý khách đã ủng hộ shop', CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Response] ([feedbackId], [sellerId], [sellerMessage], [timestamp]) VALUES (2, 2, N'Xin cảm ơn quý khách đã ủng hộ hết mình', CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[Response] ([feedbackId], [sellerId], [sellerMessage], [timestamp]) VALUES (3, 4, N'Xin cảm ơn ạ', CAST(N'2023-09-27T19:55:51.333' AS DateTime))
GO
INSERT [dbo].[Seller] ([id], [name], [phone], [profilePicture], [registeredAt], [ratings], [status], [shopId]) VALUES (2, N'Datkaa', N'0909000002', N'https://i.pinimg.com/564x/ed/b4/93/edb493abb105d4209ecfea8fbc8985d9.jpg', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(0.0 AS Decimal(2, 1)), 2, N'123')
INSERT [dbo].[Seller] ([id], [name], [phone], [profilePicture], [registeredAt], [ratings], [status], [shopId]) VALUES (4, N'King Eric', N'0909000004', N'https://i.pinimg.com/736x/22/6c/d0/226cd04172c9e62bb3cfff63fa76a128.jpg', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(0.0 AS Decimal(2, 1)), 2, N'123')
INSERT [dbo].[Seller] ([id], [name], [phone], [profilePicture], [registeredAt], [ratings], [status], [shopId]) VALUES (6, N'3rodeo', N'0909000006', N'https://i.pinimg.com/474x/ac/18/67/ac18674660d06894ba7c1871ca85d98f.jpg', CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(0.0 AS Decimal(2, 1)), 2, N'190238')
GO
INSERT [dbo].[Token] ([id], [refreshToken], [expiryTime]) VALUES (1, N'4NUe3tpnnpZ5t72NkR01OPkq0zYG7VLPHTHhu+pX7GQ=', CAST(N'2023-10-12T12:58:19.853' AS DateTime))
INSERT [dbo].[Token] ([id], [refreshToken], [expiryTime]) VALUES (6, N'dtWLw/ZDuyuGU2CE0a25nHDAakH4EB1+ZxOQb1t1DZY=', CAST(N'2023-11-14T13:34:45.240' AS DateTime))
INSERT [dbo].[Token] ([id], [refreshToken], [expiryTime]) VALUES (7, N'x7AF/lZcWu1emmp8LLqWBvSQiZ6vJdjUtpzi4VYFJ5A=', CAST(N'2023-11-01T14:37:41.227' AS DateTime))
INSERT [dbo].[Token] ([id], [refreshToken], [expiryTime]) VALUES (12, N'vsVN8AqTFESxvZ4RG7xQPsmnS9bf2PlhX04Qtf0LfTk=', CAST(N'2023-11-08T15:14:25.840' AS DateTime))
INSERT [dbo].[Token] ([id], [refreshToken], [expiryTime]) VALUES (13, N'1VWVXH25Z4S4WIas7oJE9sTLp/kV3bNZTUKk8GE4EuU=', CAST(N'2023-11-14T13:35:38.900' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[Transaction] ON 

INSERT [dbo].[Transaction] ([id], [walletId], [paymentMethod], [amount], [type], [date], [description], [status]) VALUES (1, 1, 2, CAST(20000.0 AS Decimal(12, 1)), 1, CAST(N'2023-10-11T20:41:04.237' AS DateTime), N'app_id: 2553, app_trans_id: 231010_1696931155812, mac: 1d877da5e2f7b2fe85a907a7d47a6b71cc0c82a7435f49be70a87ddea0f47c11', 1)
INSERT [dbo].[Transaction] ([id], [walletId], [paymentMethod], [amount], [type], [date], [description], [status]) VALUES (2, 7, 1, CAST(7650000.0 AS Decimal(12, 1)), 3, CAST(N'2023-10-14T13:38:24.810' AS DateTime), N'Payment for order: 17', 1)
INSERT [dbo].[Transaction] ([id], [walletId], [paymentMethod], [amount], [type], [date], [description], [status]) VALUES (3, 7, 1, CAST(1550000.0 AS Decimal(12, 1)), 3, CAST(N'2023-10-14T13:38:24.847' AS DateTime), N'Payment for order: 18', 1)
INSERT [dbo].[Transaction] ([id], [walletId], [paymentMethod], [amount], [type], [date], [description], [status]) VALUES (4, 7, 1, CAST(10000.0 AS Decimal(12, 1)), 3, CAST(N'2023-10-25T14:28:57.433' AS DateTime), N'Payment for order: 20', 1)
INSERT [dbo].[Transaction] ([id], [walletId], [paymentMethod], [amount], [type], [date], [description], [status]) VALUES (5, 7, 1, CAST(1558000.0 AS Decimal(12, 1)), 4, CAST(N'2023-11-01T15:19:19.133' AS DateTime), N'Refund for order: 18', 1)
INSERT [dbo].[Transaction] ([id], [walletId], [paymentMethod], [amount], [type], [date], [description], [status]) VALUES (6, 13, 1, CAST(24560000.0 AS Decimal(12, 1)), 3, CAST(N'2023-11-01T19:44:34.167' AS DateTime), N'Payment for order: 21', 1)
INSERT [dbo].[Transaction] ([id], [walletId], [paymentMethod], [amount], [type], [date], [description], [status]) VALUES (7, 6, 1, CAST(23823500.0 AS Decimal(12, 1)), 5, CAST(N'2023-11-07T13:36:06.497' AS DateTime), N'Done for order: 21', 1)
SET IDENTITY_INSERT [dbo].[Transaction] OFF
GO
SET IDENTITY_INSERT [dbo].[User] ON 

INSERT [dbo].[User] ([id], [name], [email], [password], [phone], [profilePicture], [gender], [dob], [role], [status], [createdAt], [updatedAt]) VALUES (1, N'Vũ Triều Dương', N'duong', N'$2a$13$rDYslDr/py8IpNoM/WXuS.azLJqYwh6.r97Xa2k3vYZ/xV3bgD.56', N'0909000001', N'https://i.pinimg.com/236x/b1/8c/b3/b18cb366c22b2a55bf8adc14cfa54468.jpg', 1, CAST(N'2023-09-27T19:55:51.333' AS DateTime), 0, 1, CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[User] ([id], [name], [email], [password], [phone], [profilePicture], [gender], [dob], [role], [status], [createdAt], [updatedAt]) VALUES (2, N'Trần Dương Phúc Đạt', N'dattdpse161459@fpt.edu.vn', N'12345678', N'0909000002', N'https://i.pinimg.com/564x/ed/b4/93/edb493abb105d4209ecfea8fbc8985d9.jpg', 1, CAST(N'2023-09-27T19:55:51.333' AS DateTime), 1, 1, CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[User] ([id], [name], [email], [password], [phone], [profilePicture], [gender], [dob], [role], [status], [createdAt], [updatedAt]) VALUES (3, N'Trần Ngọc Châu', N'chautnse161445@fpt.edu.vn', N'12345678', N'0909000003', N'https://img5.thuthuatphanmem.vn/uploads/2021/11/30/ban-bo-mia_081944018.jpg', 1, CAST(N'2023-09-27T19:55:51.333' AS DateTime), 0, 1, CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[User] ([id], [name], [email], [password], [phone], [profilePicture], [gender], [dob], [role], [status], [createdAt], [updatedAt]) VALUES (4, N'Nguyễn Huỳnh Tuấn', N'tuannhse161397@fpt.edu.vn', N'12345678', N'0909000004', N'https://i.pinimg.com/736x/22/6c/d0/226cd04172c9e62bb3cfff63fa76a128.jpg', 1, CAST(N'2023-09-27T19:55:51.333' AS DateTime), 1, 1, CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[User] ([id], [name], [email], [password], [phone], [profilePicture], [gender], [dob], [role], [status], [createdAt], [updatedAt]) VALUES (5, N'Lê Minh Đức', N'duclmse161422@fpt.edu.vn', N'12345678', N'0909000005', N'https://i.pinimg.com/564x/10/f6/4a/10f64a7cb313dc679fb6c16db4999476.jpg', 1, CAST(N'2023-09-27T19:55:51.333' AS DateTime), 0, 1, CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[User] ([id], [name], [email], [password], [phone], [profilePicture], [gender], [dob], [role], [status], [createdAt], [updatedAt]) VALUES (6, N'Hoàng Lê Gia Bảo', N'baohlgse161429@fpt.edu.vn', N'$2a$13$rDYslDr/py8IpNoM/WXuS.azLJqYwh6.r97Xa2k3vYZ/xV3bgD.56', N'0909000006', N'https://i.pinimg.com/474x/ac/18/67/ac18674660d06894ba7c1871ca85d98f.jpg', 1, CAST(N'2023-09-27T19:55:51.333' AS DateTime), 2, 1, CAST(N'2023-09-27T19:55:51.333' AS DateTime), CAST(N'2023-09-27T19:55:51.333' AS DateTime))
INSERT [dbo].[User] ([id], [name], [email], [password], [phone], [profilePicture], [gender], [dob], [role], [status], [createdAt], [updatedAt]) VALUES (7, N'string', N'a', N'$2a$13$rDYslDr/py8IpNoM/WXuS.azLJqYwh6.r97Xa2k3vYZ/xV3bgD.56', N'string    ', N'https://static.vecteezy.com/system/resources/thumbnails/001/840/618/small/picture-profile-icon-male-icon-human-or-people-sign-and-symbol-free-vector.jpg', 0, CAST(N'2023-10-10T12:12:14.327' AS DateTime), 2, 1, CAST(N'2023-10-10T19:12:18.240' AS DateTime), CAST(N'2023-10-10T19:12:18.240' AS DateTime))
INSERT [dbo].[User] ([id], [name], [email], [password], [phone], [profilePicture], [gender], [dob], [role], [status], [createdAt], [updatedAt]) VALUES (12, N'string', N'@fpt.edu.vn', N'$2a$13$rDYslDr/py8IpNoM/WXuS.azLJqYwh6.r97Xa2k3vYZ/xV3bgD.56', N'string    ', N'https://static.vecteezy.com/system/resources/thumbnails/001/840/618/small/picture-profile-icon-male-icon-human-or-people-sign-and-symbol-free-vector.jpg', 0, CAST(N'2023-10-31T07:17:20.570' AS DateTime), 1, 1, CAST(N'2023-10-31T14:17:39.743' AS DateTime), CAST(N'2023-10-31T14:17:58.367' AS DateTime))
INSERT [dbo].[User] ([id], [name], [email], [password], [phone], [profilePicture], [gender], [dob], [role], [status], [createdAt], [updatedAt]) VALUES (13, N'string', N'user@example.com', N'$2a$13$rDYslDr/py8IpNoM/WXuS.azLJqYwh6.r97Xa2k3vYZ/xV3bgD.56', N'string    ', N'https://static.vecteezy.com/system/resources/thumbnails/001/840/618/small/picture-profile-icon-male-icon-human-or-people-sign-and-symbol-free-vector.jpg', 0, CAST(N'2023-11-01T07:30:53.690' AS DateTime), 1, 1, CAST(N'2023-11-01T14:31:29.860' AS DateTime), CAST(N'2023-11-01T14:31:29.860' AS DateTime))
SET IDENTITY_INSERT [dbo].[User] OFF
GO
INSERT [dbo].[VerifyToken] ([id], [code], [expirationDate], [status]) VALUES (13, N'ymn6qhwguzf52svgu66h8nzeoj8ebhlhwpsafhwh', CAST(N'2023-11-04T14:31:30.047' AS DateTime), 1)
GO
INSERT [dbo].[Wallet] ([id], [availableBalance], [lockedBalance], [status]) VALUES (1, CAST(25020000.0 AS Decimal(12, 1)), CAST(0.0 AS Decimal(12, 1)), 1)
INSERT [dbo].[Wallet] ([id], [availableBalance], [lockedBalance], [status]) VALUES (3, CAST(50000000.0 AS Decimal(12, 1)), CAST(0.0 AS Decimal(12, 1)), 1)
INSERT [dbo].[Wallet] ([id], [availableBalance], [lockedBalance], [status]) VALUES (5, CAST(42500000.0 AS Decimal(12, 1)), CAST(0.0 AS Decimal(12, 1)), 1)
INSERT [dbo].[Wallet] ([id], [availableBalance], [lockedBalance], [status]) VALUES (6, CAST(23823500.0 AS Decimal(12, 1)), CAST(0.0 AS Decimal(12, 1)), 1)
INSERT [dbo].[Wallet] ([id], [availableBalance], [lockedBalance], [status]) VALUES (7, CAST(2348000.0 AS Decimal(12, 1)), CAST(0.0 AS Decimal(12, 1)), 1)
INSERT [dbo].[Wallet] ([id], [availableBalance], [lockedBalance], [status]) VALUES (12, CAST(0.0 AS Decimal(12, 1)), CAST(0.0 AS Decimal(12, 1)), 1)
INSERT [dbo].[Wallet] ([id], [availableBalance], [lockedBalance], [status]) VALUES (13, CAST(975440000.0 AS Decimal(12, 1)), CAST(0.0 AS Decimal(12, 1)), 1)
GO
ALTER TABLE [dbo].[Address]  WITH CHECK ADD FOREIGN KEY([customerId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Auction]  WITH CHECK ADD FOREIGN KEY([productId])
REFERENCES [dbo].[Product] ([id])
GO
ALTER TABLE [dbo].[Auction]  WITH CHECK ADD FOREIGN KEY([staffId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Bid]  WITH CHECK ADD FOREIGN KEY([auctionId])
REFERENCES [dbo].[Auction] ([id])
GO
ALTER TABLE [dbo].[Bid]  WITH CHECK ADD FOREIGN KEY([bidderId])
REFERENCES [dbo].[Buyer] ([id])
GO
ALTER TABLE [dbo].[Buyer]  WITH CHECK ADD FOREIGN KEY([id])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Feedback]  WITH CHECK ADD FOREIGN KEY([buyerId])
REFERENCES [dbo].[Buyer] ([id])
GO
ALTER TABLE [dbo].[Feedback]  WITH CHECK ADD FOREIGN KEY([productId])
REFERENCES [dbo].[Product] ([id])
GO
ALTER TABLE [dbo].[Order]  WITH CHECK ADD FOREIGN KEY([buyerId])
REFERENCES [dbo].[Buyer] ([id])
GO
ALTER TABLE [dbo].[Order]  WITH CHECK ADD FOREIGN KEY([sellerId])
REFERENCES [dbo].[Seller] ([id])
GO
ALTER TABLE [dbo].[OrderCancellation]  WITH CHECK ADD FOREIGN KEY([id])
REFERENCES [dbo].[Order] ([id])
GO
ALTER TABLE [dbo].[OrderItem]  WITH CHECK ADD FOREIGN KEY([orderId])
REFERENCES [dbo].[Order] ([id])
GO
ALTER TABLE [dbo].[OrderItem]  WITH CHECK ADD FOREIGN KEY([productId])
REFERENCES [dbo].[Product] ([id])
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD FOREIGN KEY([categoryId])
REFERENCES [dbo].[Category] ([id])
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD FOREIGN KEY([materialId])
REFERENCES [dbo].[Material] ([id])
GO
ALTER TABLE [dbo].[Product]  WITH CHECK ADD FOREIGN KEY([sellerId])
REFERENCES [dbo].[Seller] ([id])
GO
ALTER TABLE [dbo].[ProductImage]  WITH CHECK ADD FOREIGN KEY([productId])
REFERENCES [dbo].[Product] ([id])
GO
ALTER TABLE [dbo].[Response]  WITH CHECK ADD FOREIGN KEY([feedbackId])
REFERENCES [dbo].[Feedback] ([id])
GO
ALTER TABLE [dbo].[Response]  WITH CHECK ADD FOREIGN KEY([sellerId])
REFERENCES [dbo].[Seller] ([id])
GO
ALTER TABLE [dbo].[Seller]  WITH CHECK ADD FOREIGN KEY([id])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Token]  WITH CHECK ADD FOREIGN KEY([id])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Transaction]  WITH CHECK ADD FOREIGN KEY([walletId])
REFERENCES [dbo].[Wallet] ([id])
GO
ALTER TABLE [dbo].[VerifyToken]  WITH CHECK ADD FOREIGN KEY([id])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Wallet]  WITH CHECK ADD FOREIGN KEY([id])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Withdrawal]  WITH CHECK ADD FOREIGN KEY([ManagerId])
REFERENCES [dbo].[User] ([id])
GO
ALTER TABLE [dbo].[Withdrawal]  WITH CHECK ADD FOREIGN KEY([WalletId])
REFERENCES [dbo].[Wallet] ([id])
GO
