
	Private userNameReader As SqlDataReader
	Private sw As StreamWriter
	Const DrpList As String = "DropDownList"
	Const RdoList As String = "RadioButtonList"
	Private query As String
	Private writerecord As String
	Private windowsdirectory As New WindowsDirectoryAccess
	**********************************************
                    sw.WriteLine("<table cellpadding ='15'  width='100%'>")
          ows Then
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
                   ***********************************
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


End Class
