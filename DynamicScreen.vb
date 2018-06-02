Imports System.Data.SqlClient
Imports System.IO
Imports EBS
Imports System.Configuration
Imports System.Security.Cryptography
Imports System.Text
Imports System.Reflection
Imports System.Xml

Public Class DynamicScreen
	Private serverPath As String
	Private gblDirectory As String
	Private encryptedUserName As String
	Private userName As String
	Private encryptedPassWord As String
	Private passWord As String
	Private portNumber As Integer
	Private isLogged As Boolean
	Private objFTPClient As FTPClient.FTPClient
	Private company As String
	Private userFormCompany As String
	Private templateCompanyDirectory() As String
	Private templateRoleDirectory() As String
	Private fileName As String
	Private fileRoleName As String
	Private reader As SqlDataReader
	Private titleReader As SqlDataReader
	Private formatReader As SqlDataReader
	Private optionReader As SqlDataReader
	Private commandReader As SqlDataReader
	Private additionalReader As SqlDataReader
	Private userNameReader As SqlDataReader
	Private sw As StreamWriter
	Const DrpList As String = "DropDownList"
	Const RdoList As String = "RadioButtonList"
	Private query As String
	Private writerecord As String
	Private windowsdirectory As New WindowsDirectoryAccess
	Private tempFolder As String
	Private fileTarget As String
	Private I_pageName As String
	Private sCompanyDirectory As String
	Private rCompany As String
	Private sCompanyUserList As String
	Private sCompanyList As String
	Private sUserForm As String
	Private I_userRoleName As String
    Private FillQuery As String
    Dim queryCompany As String
    Dim companyReader As SqlDataReader


    Public Sub GeneratePage(ByVal I_userRole As Integer, ByVal I_pageID As Integer, ByVal I_connString As String, ByVal I_LanguageID As Integer, Optional ByVal I_companyID As Integer = 0)
        'Optional ByVal EmployeeID As Integer = 0
        Dim _dataAccess As New SqlHelper(I_connString)
        Dim _LanguageID As Integer
        Dim _sName As String
        Dim qryUserName As String
        Dim sLabel As String
        Dim CreateControl As Boolean
        Dim I_companyName As String
        _LanguageID = CType(I_LanguageID, Integer)
        Try

            'get company name
            'I_companyID
            If I_companyID = 0 Then
                I_companyName = "0"
            Else
                queryCompany = "SELECT Name FROM CompanyInfo"
                companyReader = _dataAccess.ExecuteReader(queryCompany)
                If companyReader.HasRows Then
                    companyReader.Read()
                    I_companyName = Replace(CType(companyReader("Name"), String), " ", "")
                Else
                    I_companyName = "0"
                End If
            End If

            'Stored Proc For Member Controls
            Dim memberControlParaList(1) As SqlParameter

            memberControlParaList(0) = New SqlParameter
            With memberControlParaList(0)
                .ParameterName = "@PageID"
                .Value = I_pageID
                .SqlDbType = SqlDbType.VarChar
            End With
            memberControlParaList(1) = New SqlParameter
            With memberControlParaList(1)
                .ParameterName = "@UserRoleID"
                .Value = I_userRole
                .SqlDbType = SqlDbType.Int
            End With

            reader = _dataAccess.ExecuteReader(I_connString, CommandType.StoredProcedure, "OES_SP_GetMemberControls", memberControlParaList)

            'If controls exists for the page then
            If reader.HasRows Then

                'Get title for the page that is to be generated
                query = "SELECT Title,Name FROM Pages P" & _
                " WHERE P.ID=" & I_pageID
                titleReader = _dataAccess.ExecuteReader(query)
                If titleReader.HasRows Then
                    titleReader.Read()
                    I_pageName = Replace(CType(titleReader("Name"), String), " ", "")
                End If
                'Create File in Windows Temp Folder
                fileName = I_pageName & ".aspx"
                tempFolder = windowsdirectory.GetTempDirectory()
                fileTarget = tempFolder + fileName
                sw = New StreamWriter(fileTarget)

                'Add basic tags to the page
                sw.WriteLine("<%@ Page Language='vb' AutoEventWireup='false' Codebehind='EmployeeProfile.aspx.vb' Inherits='OnlineEnrollment.EmployeeProfile'%>")
                'sw.WriteLine("<%@ Page Language='vb' AutoEventWireup='true' Codebehind='" & I_pageName & ".aspx.vb' %>")
                sw.WriteLine("<!DOCTYPE HTML PUBLIC '-//W3C//DTD HTML 4.0 Transitional//EN'>")
                sw.WriteLine("<HTML>")
                sw.WriteLine("<HEAD>")
                sw.WriteLine("<title>" & I_pageName & "</title>")
                'sw.WriteLine("<LINK href='../DynamicScreenGeneration.css' type='text/css' rel='Stylesheet'>")
                sw.WriteLine("<LINK href='../../StyleSheet/OnlineEnrollment.css' type='text/css' rel='Stylesheet'>")
                sw.WriteLine("<meta name='GENERATOR' content='Microsoft Visual Studio .NET 7.1'>")
                sw.WriteLine("<meta name='CODE_LANGUAGE' content='Visual Basic .NET 7.1'>")
                sw.WriteLine("<meta name='vs_defaultClientScript' content='JavaScript'>")
                sw.WriteLine("</HEAD>")
                sw.WriteLine("<body MS_POSITIONING='GridLayout'>")
                sw.WriteLine("<form id=Frm" & I_pageName & " method='post' runat='server'>")

                'If title exists then add title to the presentation page
                If titleReader.HasRows Then
                    'titleReader.Read()
                    'Added for formating left alignment
                    '******************************************************
                    sw.WriteLine("<table cellpadding ='15'  width='100%'>")
                    sw.WriteLine("<tr>")
                    sw.WriteLine("<td>")
                    '******************************************************
                    sw.WriteLine("<table width='100%'>")
                    sw.WriteLine("<tr>")
                    sw.WriteLine("<td colspan='2'>")
                    sw.WriteLine("<asp:label id='lblTitle'" & " Text='" & CType(titleReader("Title"), String) & "' runat='server' CssClass='PageTitle'" & " /><hr>")
                    sw.WriteLine("</td>")
                    sw.WriteLine("</tr>")
                    sw.WriteLine("<tr>")
                    sw.WriteLine("<td>")
                    'sw.WriteLine("</table>")
                End If

                query = " Select dbo.FormatMemberDocument.Description as Note,dbo.Role.Name as UserName " & _
                " FROM         dbo.FormatMemberDocument INNER JOIN " & _
                " dbo.FormatMemberMessages ON dbo.FormatMemberDocument.FormatMemberMessageID = dbo.FormatMemberMessages.ID INNER JOIN " & _
                " dbo.Role ON dbo.FormatMemberDocument.RoleID = dbo.Role.ID " & _
                " WHERE (dbo.Role.ID = " & I_userRole & ") AND dbo.FormatMemberDocument.PageID = " & I_pageID & ""
                '" WHERE (dbo.Role.Name = '" & I_userRole & "') AND dbo.FormatMemberDocument.PageID = " & I_pageID & ""

                formatReader = _dataAccess.ExecuteReader(query)

                'If note exists then add note to the presentation page
                If formatReader.HasRows Then
                    formatReader.Read()
                    sw.WriteLine("<table>")
                    sw.WriteLine("<tr>")
                    sw.WriteLine("<td colspan='2'>")
                    sw.WriteLine(formatReader("Note"))
                    sw.WriteLine("</td>")
                    sw.WriteLine("</tr>")
                    'sw.WriteLine("</table>")
                    sw.WriteLine("<tr>")
                    sw.WriteLine("<td colspan='2'>")

                    I_userRoleName = CType(formatReader("UserName"), String)
                Else
                    'Get username from role table
                    qryUserName = "SELECT Name FROM Role " & _
                    " WHERE Role.ID = " & I_userRole
                    userNameReader = _dataAccess.ExecuteReader(qryUserName)
                    If userNameReader.HasRows Then
                        userNameReader.Read()
                        I_userRoleName = CType(userNameReader("Name"), String)
                    Else
                        I_userRoleName = "Anonymous"
                    End If

                    sw.WriteLine("<table>")
                    sw.WriteLine("<tr>")
                    sw.WriteLine("<td colspan='2'>")
                    sw.WriteLine("</td>")
                    sw.WriteLine("</tr>")
                    sw.WriteLine("<tr>")
                    sw.WriteLine("<td colspan='2'>")

                End If

                'Add validation summary to the page
                'sw.WriteLine("<table width='75%'>")
                'sw.WriteLine("<tr>")
                'sw.WriteLine("<td>")
                sw.WriteLine("<asp:validationsummary id='ValidationSummaryControl' runat='server'></asp:validationsummary>")
                sw.WriteLine("</td>")
                sw.WriteLine("</tr>")
                'sw.WriteLine("</table>")
                'sw.WriteLine("<table width='75%'>")
                While reader.Read()
                    sw.WriteLine("<tr>")

                    'Add labels for the controls to the presentation page
                    sw.WriteLine("<td width='30%'>")
                    If String.Compare(CType(reader("LabelText"), String), sLabel) <> 0 Then
                        If String.Compare(CType(reader("FieldType"), String), "GridButton") <> 0 Then
                            If String.Compare(CType(reader("FieldType"), String), "SubTitle") = 0 Then
                                sw.WriteLine("</br><asp:label id=" & Replace(CType(reader("LabelText"), String), " ", "") & " Text='" & CType(reader("LabelText"), String) & "' runat='server' CssClass='label'" & " />")
                            Else
                                sw.WriteLine("<asp:label id=" & Replace(CType(reader("LabelText"), String), " ", "") & " Text='" & CType(reader("LabelText"), String) & "' runat='server' CssClass='default'" & " />")
                            End If
                            sLabel = CType(reader("LabelText"), String)
                            CreateControl = True
                        Else
                            CreateControl = True
                        End If
                    Else
                        CreateControl = False
                    End If


                    sw.WriteLine("</td>")

                    'If the control is wither dropdownlist or radiolist
                    If CType(reader("FieldType"), String) = DrpList Or CType(reader("FieldType"), String) = RdoList Then

                        'If the UDF is not parameterised
                        If CType(reader("IsParameterised"), Boolean) = True Then
                            'If Not IsNothing(reader("query")) Then
                            If CType(reader("query"), String) = "State" Or CType(reader("query"), String) = "Country" Or CType(reader("query"), String) = "Class1" Or CType(reader("query"), String) = "Class2" Or CType(reader("query"), String) = "Class3" Or CType(reader("query"), String) = "Class4" Or CType(reader("query"), String) = "PTOCategory" Then
                                FillQuery = "SELECT ID,Name FROM " & reader("query").ToString()
                            Else
                                _sName = "Name" & _LanguageID
                                FillQuery = "SELECT ID," & _sName & " FROM " & reader("query").ToString()
                            End If

                            'Get the records to populate the control
                            'optionReader = _dataAccess.ExecuteReader(I_connString, CommandType.Text, "SELECT * FROM " & reader("query").ToString())
                            optionReader = _dataAccess.ExecuteReader(I_connString, CommandType.Text, FillQuery)
                            If optionReader.HasRows Then
                                sw.WriteLine("<td>")
                                sw.WriteLine(reader("FieldText"))
                                While optionReader.Read
                                    If CType(reader("query"), String) = "State" Or CType(reader("query"), String) = "Country" Or CType(reader("query"), String) = "Class1" Or CType(reader("query"), String) = "Class2" Or CType(reader("query"), String) = "Class3" Or CType(reader("query"), String) = "Class4" Or CType(reader("query"), String) = "PTOCategory" Then
                                        sw.WriteLine("<asp:ListItem Value=" & Replace(CType(optionReader("ID"), String), " ", "") & ">" & CType(optionReader("Name"), String) & " </asp:ListItem>")
                                    Else
                                        sw.WriteLine("<asp:ListItem Value=" & Replace(CType(optionReader("ID"), String), " ", "") & ">" & CType(optionReader(_sName), String) & " </asp:ListItem>")
                                    End If
                                End While
                                If CType(reader("FieldType"), String) = DrpList Then
                                    sw.WriteLine("</asp:DropDownList>")
                                Else
                                    sw.WriteLine("</asp:RadioButtonList>")
                                End If
                                sw.WriteLine("</td>")
                                sw.WriteLine("</tr>")
                            End If
                        Else
                            sw.WriteLine("<td>")
                            sw.WriteLine(reader("FieldText"))
                            If CType(reader("FieldType"), String) = DrpList Then
                                sw.WriteLine("</asp:DropDownList>")
                            Else
                                sw.WriteLine("</asp:RadioButtonList>")
                            End If
                            sw.WriteLine("</td>")
                            sw.WriteLine("</tr>")
                        End If
                        'End If
                    Else

                        'Add feteched controls to the presentation page

                        If (String.Compare(CType(reader("FieldType"), String), "SubTitle") <> 0) Then
                            If CreateControl = True Then 'For validation controls 
                                If String.Compare(CType(reader("FieldType"), String), "GridButton") <> 0 Then
                                    sw.WriteLine("<td>")
                                    sw.WriteLine(reader("FieldText"))
                                    sw.WriteLine("</td>")
                                Else
                                    sw.WriteLine("<td Align='left'>")
                                    sw.WriteLine(reader("FieldText"))
                                    sw.WriteLine("</td>")
                                End If
                            End If
                        End If
                        If CType(reader("Validator"), String) <> "No Validator" Then
                            sw.WriteLine("<td>")
                            sw.WriteLine(reader("Validator"))
                            sw.WriteLine("</td>")
                        End If

                        sw.WriteLine("</tr>")
                    End If

                End While
                'EXECUTE OES_SP_GetAdditionalControls  @PageName
                Dim additionalControlParaList(0) As SqlParameter

                additionalControlParaList(0) = New SqlParameter
                With additionalControlParaList(0)
                    .ParameterName = "@PageID"
                    .Value = I_pageID
                    .SqlDbType = SqlDbType.VarChar
                End With

                additionalReader = _dataAccess.ExecuteReader(I_connString, CommandType.StoredProcedure, "OES_SP_GetAdditionalControls", additionalControlParaList)

                'Add additional controls to the presentation page
                If additionalReader.HasRows Then

                    'Add the sub title for additional fields
                    sw.WriteLine("<tr><td></br><asp:Label id='lblAdditionalInformation' Text='Additional Information' CSSClass='label' runat='server' /></td></tr>")
                    While additionalReader.Read

                        'Add label for the additional field
                        sw.WriteLine("<tr>")
                        sw.WriteLine("<td>")
                        sw.WriteLine("<asp:label id=" & "lbl" & Replace(CType(additionalReader("Label"), String), " ", "") & " Text='" & CType(additionalReader("Label"), String) & "' runat='server' CSSClass='Default'" & " />")
                        sw.WriteLine("</td>")

                        'Add control for the additional field
                        sw.WriteLine("<td>")
                        sw.WriteLine(additionalReader("FieldText"))
                        sw.WriteLine("</td>")

                        'Add validation control for the additional field
                        If CType(additionalReader("Validator"), String) <> "No Validator" Then
                            sw.WriteLine("<td>")
                            sw.WriteLine(additionalReader("Validator"))
                            sw.WriteLine("</td>")
                        End If

                        If CType(additionalReader("RegularExpressionValidator"), String) <> "No Validator" Then
                            sw.WriteLine("<td>")
                            sw.WriteLine(additionalReader("RegularExpressionValidator"))
                            sw.WriteLine("</td>")
                        End If
                        sw.WriteLine("</tr>")
                    End While
                End If
                'sw.WriteLine("</table>")

                'Get command buttons for the page as per the sequence number
                'query = " SELECT fTags.StartTag + '''' + f.FieldName + '''' + ' ' + " & _
                query = "SELECT fTags.StartTag + '''' + f.FieldName + '''' + ' '+ f.FieldText +  ' ' +" & _
                 " fTags.EndTag as FieldText " & _
                 " FROM Fields f,FieldType fType,FieldTags fTags " & _
                 " WHERE(f.PageID = " & I_pageID & ") " & _
                 " AND fType.Name='Button' " & _
                 " AND fType.ID=f.FieldTypeID " & _
                 " AND fTags.FieldTypeID=f.FieldTypeID " & _
                 " Order by f.SequenceNumber asc "

                commandReader = _dataAccess.ExecuteReader(query)

                'Add commands to the presentation page
                If commandReader.HasRows Then
                    'sw.WriteLine("<table>")
                    sw.WriteLine("<tr>")
                    sw.WriteLine("<td>&nbsp;")
                    sw.WriteLine("</td>")
                    sw.WriteLine("</tr>")
                    sw.WriteLine("<tr>")
                    sw.WriteLine("<td>")
                    While commandReader.Read
                        'sw.WriteLine("<td>")
                        sw.WriteLine(commandReader("FieldText"))
                        'sw.WriteLine("</td>")
                    End While
                    sw.WriteLine("</td>")
                    sw.WriteLine("</tr>")
                End If

                'sw.WriteLine("</table>")
                'Added for formating left alignment
                '******************************************************
                sw.WriteLine("</td>")
                sw.WriteLine("</tr>")
                sw.WriteLine("</table>")
                '******************************************************
                sw.WriteLine("</form>")
                sw.WriteLine("</body>")
                sw.WriteLine("</HTML>")
            End If

            'Close the file
            sw.Close()

            'Upload the page to FTP server
            If I_companyName = "0" Then
                Call UploadFile(I_userRoleName, I_pageName, fileTarget)
            Else
                Call UploadFile(I_companyName, I_userRoleName, I_pageName, fileTarget)
            End If

            'Delete the file from temp folder
            File.Delete(fileTarget)

        Catch ex As Exception
            Throw ex
        Finally
            'Dispose the objects
            sw = Nothing
            reader = Nothing
            titleReader = Nothing
            formatReader = Nothing
            optionReader = Nothing
            commandReader = Nothing
            additionalReader = Nothing
            _dataAccess = Nothing

        End Try
    End Sub

    'Uploading the page to FTP server in respective folder according to userrole
    'Overloaded function UploadFile for uploading pages to FTPServer - userforms-company-companyA-users
    Private Sub UploadFile(ByVal I_companyName As String, ByVal I_userRole As String, ByVal I_pageName As String, ByVal I_fileName As String)
        Try
            'Initializing Application Configuration Settings
            InitializeConfiguration()
            'Connecting to FTP server
            objFTPClient = New FTPClient.FTPClient(serverPath, gblDirectory, userName, passWord, portNumber)

            If (objFTPClient.Login() = True) Then
                isLogged = True
                'Changing to Global Directory
                If (objFTPClient.ChangeDirectory(gblDirectory) = True) Then
                    If (objFTPClient.ChangeDirectory(sUserForm) = True) Then
                        If (objFTPClient.ChangeDirectory(sCompanyList) = True) Then
                            If (objFTPClient.ChangeDirectory(I_companyName) = True) Then
                                'Check For User Role
                                If (objFTPClient.ChangeDirectory(I_userRole) = True) Then
                                    objFTPClient.SetBinaryMode(True)
                                    'Uploading the Template File
                                    objFTPClient.UploadFile(I_fileName)
                                Else
                                    objFTPClient.CreateDirectory(I_userRole)
                                    If (objFTPClient.ChangeDirectory(I_userRole) = True) Then
                                        objFTPClient.SetBinaryMode(True)
                                        'Uploading the Template File
                                        objFTPClient.UploadFile(I_fileName)
                                    End If
                                End If
                            Else
                                objFTPClient.CreateDirectory(I_companyName)
                                If (objFTPClient.ChangeDirectory(I_companyName) = True) Then
                                    'Check For User Role
                                    If (objFTPClient.ChangeDirectory(I_userRole) = True) Then
                                        objFTPClient.SetBinaryMode(True)
                                        'Uploading the Template File
                                        objFTPClient.UploadFile(I_fileName)
                                    Else
                                        objFTPClient.CreateDirectory(I_userRole)
                                        If (objFTPClient.ChangeDirectory(I_userRole) = True) Then
                                            objFTPClient.SetBinaryMode(True)
                                            'Uploading the Template File
                                            objFTPClient.UploadFile(I_fileName)
                                        End If
                                    End If

                                End If
                            End If
                        End If

                    End If
                Else
                End If
            End If
        Catch ex As Exception
        Finally
            'For Closing ftp connection 
            If (isLogged = True) Then
                objFTPClient.CloseConnection()
            End If
        End Try
    End Sub
    'Overloaded function UploadFile for uploading pages to FTPServer - userforms-company-companyA-users
    Private Sub UploadFile(ByVal I_userRole As String, ByVal I_pageName As String, ByVal I_fileName As String)

        Try
            'Connecting to FTP server
            InitializeConfiguration()
            objFTPClient = New FTPClient.FTPClient(serverPath, gblDirectory, userName, passWord, portNumber)
            Dim sCompanyUserList As String
            If (objFTPClient.Login() = True) Then
                isLogged = True
                If (objFTPClient.ChangeDirectory(gblDirectory) = True) Then
                    If (objFTPClient.ChangeDirectory(sUserForm) = True) Then
                        If (objFTPClient.ChangeDirectory(sCompanyList) = True) Then
                            templateCompanyDirectory = objFTPClient.GetFileList(company)
                            'Getting all files from remote directory							
                            For Each sCompanyDirectory In templateCompanyDirectory
                                If Not IsNothing(sCompanyDirectory) And sCompanyDirectory <> "" Then
                                    sCompanyDirectory = Trim(Mid(sCompanyDirectory, 1, Len(sCompanyDirectory) - 1))
                                    sCompanyUserList = sCompanyList & "/" & sCompanyDirectory
                                    If (objFTPClient.ChangeDirectory(sCompanyDirectory) = True) Then
                                        templateRoleDirectory = objFTPClient.GetFileList(company)
                                        For Each fileRoleName In templateRoleDirectory
                                            If Not IsNothing(fileRoleName) And fileRoleName <> "" Then
                                                fileRoleName = Mid(fileRoleName, 1, Len(fileRoleName) - 1)
                                                If Trim(UCase(fileRoleName)) = Trim(UCase(I_userRole)) Then
                                                    If (objFTPClient.ChangeDirectory(fileRoleName) = True) Then
                                                        objFTPClient.SetBinaryMode(True)
                                                        objFTPClient.UploadFile(I_fileName)
                                                        objFTPClient.ChangeDirectory(sCompanyUserList)
                                                    End If
                                                End If
                                            End If
                                        Next
                                        'Else
                                        'objFTPClient.CreateDirectory(fileName)
                                        'If (objFTPClient.ChangeDirectory(fileName) = True) Then
                                        'templateRoleDirectory = objFTPClient.GetFileList(company)
                                        '		For Each fileRoleName In templateRoleDirectory
                                        '			If (objFTPClient.ChangeDirectory(fileRoleName) = True) Then
                                        '					objFTPClient.SetBinaryMode(True)
                                        '					objFTPClient.UploadFile(I_fileName)
                                        '			end if
                                        '		Next	
                                        'End if
                                    End If
                                End If
                                objFTPClient.ChangeDirectory(sCompanyList)
                            Next
                        End If
                    End If
                End If

            End If

        Catch ex As Exception
        Finally
            'For Closing ftp connection 
            If (isLogged = True) Then
                objFTPClient.CloseConnection()
            End If
        End Try
    End Sub
    Private Sub InitializeConfiguration()
        'These settings were retrieved from the web.config file form onlineenrollment
        'serverPath = ConfigurationSettings.AppSettings("ftpConn")
        'portNumber = CType(ConfigurationSettings.AppSettings("PORT"), Integer)
        ''Fetching GlobalDirectory from App.Config file
        'gblDirectory = ConfigurationSettings.AppSettings("gblDirectory")
        'userFormCompany = ConfigurationSettings.AppSettings("userForm")
        'rCompany = ConfigurationSettings.AppSettings("Company")

        Dim sPath As String
        Dim ConfigFileName As String
        ConfigFileName = "Benefits.dll.config"

        sPath = System.AppDomain.CurrentDomain.BaseDirectory()
        sPath = Path.GetFullPath(ConfigFileName)
        sPath = Path.GetDirectoryName(ConfigFileName)
        'To get the path of the current assembly
        'sPath = ([Assembly].GetCallingAssembly.GetName(False).CodeBase.Replace("file:///", "").Replace("/", "\\") + ".config")
        'Fetching FTP settings from App.Config file
        serverPath = ReadConfig("ftpConn")
        portNumber = CType(ReadConfig("PORT"), Integer)
        gblDirectory = ReadConfig("gblDirectory")
        userFormCompany = ReadConfig("userForm")
        rCompany = ReadConfig("Company")
        sUserForm = ReadConfig("sUserForm")
        sCompanyList = ReadConfig("sCompanyList")
        sCompanyUserList = ReadConfig("sCompanyUserList")

        'Fetching sCompanyList from Benefits App.Config
        sCompanyList = ConfigurationSettings.AppSettings("sCompanyList")
        sCompanyUserList = ConfigurationSettings.AppSettings("sCompanyUserList")

        'Fetching Encrypted Username from App.Config
        encryptedUserName = ConfigurationSettings.AppSettings("encryptUserName")
        'Decryting Username 
        userName = DecryptSHA(encryptedUserName)
        'Fetching Encrypted Password from App.Config file
        encryptedPassWord = ConfigurationSettings.AppSettings("encryptPassWord")
        'Decrypting Password
        passWord = DecryptSHA(encryptedPassWord)
    End Sub
    'Description:     This Function can uses for Encryption of string and text. 
    'Arguments:       [IN]encryptString          To Encrypt the string.
    'Return Value:    Decrypted Data   

    Private Function EncryptSHA(ByVal encryptString As String) As String
        Dim encrypt As String
        Dim shaM As New SHA1Managed

        Try
            'For Converting string to Byte 
            Convert.ToBase64String(shaM.ComputeHash(Encoding.ASCII.GetBytes(encryptString)))
            Dim encryptData() As Byte = ASCIIEncoding.ASCII.GetBytes(encryptString)
            encrypt = Convert.ToBase64String(encryptData)
            EncryptSHA = encrypt
        Catch ex As Exception
            Return ex.Message
        Finally
            encrypt = Nothing
        End Try

    End Function
    'Description:     This Function can uses for Decryption of bytes to string.
    'Arguments:       [IN]decryptString          To Decrypt the string.
    'Return Value:    Decrypted Data        
    Private Function DecryptSHA(ByVal decryptString As String) As String
        Dim decrypt As String
        Try
            'For Converting  Byte to string 
            Dim decryptData() As Byte = Convert.FromBase64String(Trim(decryptString))
            decrypt = ASCIIEncoding.ASCII.GetString(decryptData)
            DecryptSHA = decrypt
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function
    'Function accepts the key and return  its value from app.dll.config
    Private Function ReadConfig(ByVal sKey As String) As String
        Dim sPath As String
        Dim sPath1 As String
        Dim sFileName As String
        Dim sTargetFile As String
        sFileName = "\Benefits.dll.config"
        sPath = ([Assembly].GetCallingAssembly.GetName(False).CodeBase.Replace("file:///", "") + ".config")
        'sPath = "D:\EBS\BenCentral\Benefits\" & sFileName
        'sPath = "Benefits.dll.config"
        sTargetFile = sPath

        If File.Exists(sTargetFile) Then
            'load file into xml doc
            Dim doc As XmlDocument = New XmlDocument
            Try
                doc.Load(sTargetFile)
                'check for nodes
                If doc.HasChildNodes Then
                    ' for each app setting
                    For Each node As XmlNode In doc.SelectNodes("/configuration/appSettings/add")
                        ' if a key attribute exists
                        If (Not (node.Attributes("key")) Is Nothing) Then
                            If node.Attributes("key").Value = sKey Then
                                Return Convert.ToString(node.Attributes("value").Value)
                            End If
                        End If
                    Next
                End If
            Catch ex As Exception
                'Could not load '{0}' into an XML document.", path
                Return String.Empty
            End Try

        End If
    End Function

End Class
