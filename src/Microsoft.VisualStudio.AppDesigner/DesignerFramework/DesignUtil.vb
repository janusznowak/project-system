' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.Shell.Interop
Imports System.Runtime.InteropServices


Namespace Microsoft.VisualStudio.Editors.AppDesDesignerFramework

    '**************************************************************************
    ';DesignUtil
    '
    'Remarks:
    '   This class contains utility methods used in the DesignerFramework.
    '   This class is converted from <DD>\wizard\vsdesigner\designer\microsoft\vsdesigner\data\DesignUtil.cs
    '**************************************************************************
    Friend NotInheritable Class DesignUtil

        '= Public =============================================================

        '**************************************************************************
        ';ReportError
        '
        'Summary:
        '   Displays an error message box with the specified error message.
        'Params:
        '   ServiceProvider: The IServiceProvider, used to get devenv shell as the parent of the message box.
        '   ErrorMessage: The text to display in the message box.
        '**************************************************************************
        Public Overloads Shared Sub ReportError(ServiceProvider As IServiceProvider, ErrorMessage As String)
            ReportError(ServiceProvider, ErrorMessage, Nothing)
        End Sub 'ReportError

        '**************************************************************************
        ';ReportError
        '
        'Summary:
        '   Displays an error message box with the specified error message and help link.
        'Params:
        '   ServiceProvider: The IServiceProvider, used to get devenv shell as the parent of the message box.
        '   ErrorMessage: The text to display in the message box.
        '   HelpLink: Link to the help topic for this message box.
        '**************************************************************************
        Public Overloads Shared Sub ReportError(ServiceProvider As IServiceProvider, ErrorMessage As String, _
                HelpLink As String)


            DesignerMessageBox.Show(ServiceProvider, ErrorMessage, GetDefaultCaption(ServiceProvider), _
                    MessageBoxButtons.OK, MessageBoxIcon.Error, HelpLink:=HelpLink)
        End Sub 'ReportError

        '**************************************************************************
        ';ShowWarning
        '
        'Summary:
        '   Displays a warning message box with the specified error message.
        'Params:
        '   ServiceProvider: The IServiceProvider, used to get devenv shell as the parent of the message box.
        '   Message: The text to display in the message box.
        '**************************************************************************
        Public Shared Sub ShowWarning(ServiceProvider As IServiceProvider, Message As String)
            DesignerMessageBox.Show(ServiceProvider, Message, GetDefaultCaption(ServiceProvider), MessageBoxButtons.OK, _
                    MessageBoxIcon.Warning)
        End Sub 'ShowWarning

        '**************************************************************************
        ';ShowMessage
        '
        'Summary:
        '   Displays a message box with specified text, caption, buttons and icon.
        'Params:
        '   ServiceProvider: The IServiceProvider, used to get devenv shell as the parent of the message box.
        '   Message: The text to display in the message box.
        '   Caption: The text to display in the title bar of the message box.
        '   Buttons: One of the MessageBoxButtons values that specifies which buttons to display in the message box.
        '   Icon: One of the MessageBoxIcon values that specifies which icon to display in the message box.
        'Returns:
        '   One of the DialogResult values.
        '**************************************************************************
        Public Overloads Shared Function ShowMessage(ServiceProvider As IServiceProvider, Message As String, _
                Caption As String, Buttons As MessageBoxButtons, Icon As MessageBoxIcon) As DialogResult
            Return DesignerMessageBox.Show(ServiceProvider, Message, Caption, Buttons, Icon)
        End Function 'ShowMessage

        '**************************************************************************
        ';ShowMessage
        '
        'Summary:
        '   Displays a message box with specified text, caption, buttons, icons and default button.
        'Params:
        '   ServiceProvider: The IServiceProvider, used to get devenv shell as the parent of the message box.
        '   Message: The text to display in the message box.
        '   Caption: The text to display in the title bar of the message box.
        '   Buttons: One of the MessageBoxButtons values that specifies which buttons to display in the message box.
        '   Icon: One of the MessageBoxIcon values that specifies which icon to display in the message box.
        '   DefaultButton: One of the MessageBoxDefaultButton values that specifies the default button of the message box.
        'Returns:
        '   One of the DialogResult values.
        '**************************************************************************
        Public Overloads Shared Function ShowMessage(ServiceProvider As IServiceProvider, Message As String, _
                        Caption As String, Buttons As MessageBoxButtons, Icon As MessageBoxIcon, _
                        DefaultButton As MessageBoxDefaultButton) As DialogResult
            Return DesignerMessageBox.Show(ServiceProvider, Message, Caption, Buttons, Icon, DefaultButton)
        End Function 'ShowMessage

        ''' <summary>
        ''' Get the default caption from IVsUIShell, or fall back to localized resource
        ''' </summary>
        ''' <param name="sp"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetDefaultCaption(sp As IServiceProvider) As String
            Dim caption As String = ""
            Dim uiShell As IVsUIShell = Nothing
            If sp IsNot Nothing Then
                uiShell = DirectCast(sp.GetService(GetType(IVsUIShell)), IVsUIShell)
            End If

            If uiShell Is Nothing OrElse AppDesInterop.NativeMethods.Failed(uiShell.GetAppName(caption)) Then
                caption = My.Resources.Designer.DFX_Error_Default_Caption
            End If

            Return caption
        End Function

        ''' <summary>
        ''' Show help (if at all possible) swallowing any COM exceptions
        ''' </summary>
        ''' <param name="ServiceProvider"></param>
        ''' <param name="keyword"></param>
        ''' <remarks></remarks>
        Public Shared Sub DisplayTopicFromF1Keyword(ServiceProvider As IServiceProvider, keyword As String)
            If ServiceProvider Is Nothing Then
                Debug.Fail("NULL serviceprovider - can't show help!")
                Return
            End If

            If keyword Is Nothing Then
                Debug.Fail("NULL help keyword - can't show help!")
                Return
            End If

            Dim vshelp As VSHelp.Help = CType(ServiceProvider.GetService(GetType(VSHelp.Help)), VSHelp.Help)

            If vshelp Is Nothing Then
                Debug.Fail("Failed to get VSHelp.Help service from given service provider - can't show help!")
                Return
            End If

            Try
                vshelp.DisplayTopicFromF1Keyword(keyword)
            Catch ex As COMException
                ' DisplayTopicFromF1Keyword may throw COM exceptions even though dexplore shows the appropriate error message
                Debug.Assert(Marshal.GetHRForException(ex) = &H80040305, String.Format("Unknown COM Exception {0} when trying to show help topic {1}", ex, keyword))
            End Try
        End Sub

        '**************************************************************************
        ';SetFontStyles
        '
        'Summary:
        '   Iterate all controls on a form and make them recreate their fonts with the desired font style.
        'Params:
        '   TopControl: The top-level control containing other controls.
        'Remarks:
        '   This method should be called especially in the OnFontChanged handler. 
        '   This way, when the VS shell font is given to us (it changes) then controls that have a different style 
        '       of the font (bolded for example) will recreate their font and use the VS shell font but bolded.
        '**************************************************************************
        Public Overloads Shared Sub SetFontStyles(TopControl As Control)
            SetFontStyles(TopControl, TopControl, TopControl.Font)
        End Sub 'SetFontStyles

        '= PRIVATE ============================================================

        '**************************************************************************
        ';SetFontStyles
        '
        'Summary:
        '   Recursive method to set the fonts of all the controls on a form.
        'Params:
        '   TopControl: The top-level control containing other controls. Each child control 
        '       will compare their fonts with this control to know whether their styles are different.
        '   Parent: The parent control used to iterate through all the control.
        '   ReferenceFont: The font to set to.
        '**************************************************************************
        Private Overloads Shared Sub SetFontStyles(TopControl As Control, Parent As Control, ReferenceFont As Font)
            For Each ChildControl As Control In Parent.Controls
                If Not (ChildControl.Controls Is Nothing) AndAlso ChildControl.Controls.Count > 0 Then
                    SetFontStyles(TopControl, ChildControl, ReferenceFont)
                End If

                If Not ChildControl.Font.Equals(TopControl.Font) Then
                    ChildControl.Font = New Font(ReferenceFont, ChildControl.Font.Style)
                End If
            Next ChildControl
        End Sub 'SetFontStyles


        ''' <summary>
        ''' Gets the signed hi word of an IntPtr
        ''' </summary>
        ''' <param name="Number">The IntPtr to get the word from</param>
        ''' <returns>The signed hi word</returns>
        ''' <remarks></remarks>
        Public Shared Function SignedHiWord(Number As IntPtr) As Integer
            Return (CType(Number, Integer) >> 16) And &HFFFF
        End Function


        ''' <summary>
        ''' Gets the signed lo word of an IntPtr
        ''' </summary>
        ''' <param name="Number">The IntPtr to get the word from</param>
        ''' <returns>The signed lo word</returns>
        ''' <remarks></remarks>
        Public Shared Function SignedLoWord(Number As IntPtr) As Integer
            Return (CType(Number, Integer) And &HFFFF)
        End Function


        ''' <summary>
        ''' Calculate the event args for raising the context menu show event for a control.
        ''' </summary>
        ''' <param name="m">Window's message.</param>
        ''' <returns>The context event args to use for raising the event.</returns>
        ''' <remarks></remarks>
        Public Shared Function GetContextMenuMouseEventArgs(Control As Control, ByRef m As Message) As MouseEventArgs
            Dim x As Integer = SignedLoWord(m.LParam)
            Dim y As Integer = SignedHiWord(m.LParam)

            ' Shift-F10 or Context Menu keyboard key will result in LParam being -1.
            If m.LParam.ToInt64 = -1 Then
                Dim p As Point = Cursor.Position
                x = p.X
                y = p.Y
            End If

            'CONSIDER: If mouse is not in client area, don't show the context menu. 
            Return New MouseEventArgs(MouseButtons.Right, 1, x, y, 0)
        End Function

        ''' <summary>
        ''' Generate a valid language independent namespace name. Differs from GenerateValidLanguageIndependentIdentifier
        ''' by allowing "." embedded in the string and zero length strings...
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GenerateValidLanguageIndependentNamespace(value As String) As String
            If value = "" Then
                Return value
            Else
                Dim subStrings() As String = value.Split(New Char() {"."c})
                For index As Integer = 0 To subStrings.Length - 1
                    subStrings(index) = GenerateValidLanguageIndependentIdentifier(subStrings(index))
                Next
                Return String.Join(".", subStrings)
            End If
        End Function

        ''' <summary>
        ''' Generate a valid language independent identifier from the given string value
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks>Will throw an ArgumentException if it fails</remarks>
        Public Shared Function GenerateValidLanguageIndependentIdentifier(value As String) As String
            Const replacementChar As Char = "_"c

            If CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(value) Then
                Return value
            End If

            Dim chars() As Char = value.ToCharArray()

            If chars.Length = 0 Then
                Throw AppDesCommon.CreateArgumentException("value")
            End If

            Dim result As New System.Text.StringBuilder

            ' First char cannot be a number
            If (Char.GetUnicodeCategory(chars(0)) = System.Globalization.UnicodeCategory.DecimalDigitNumber) Then
                result.Append(replacementChar)
            End If

            ' each char must be Lu, Ll, Lt, Lm, Lo, Nd, Mn, Mc, Pc
            ' 
            For Each ch As Char In chars
                Dim uc As Globalization.UnicodeCategory = Char.GetUnicodeCategory(ch)
                Select Case uc
                    Case Globalization.UnicodeCategory.UppercaseLetter, _
                        Globalization.UnicodeCategory.LowercaseLetter, _
                        Globalization.UnicodeCategory.TitlecaseLetter, _
                        Globalization.UnicodeCategory.ModifierLetter, _
                        Globalization.UnicodeCategory.OtherLetter, _
                        Globalization.UnicodeCategory.DecimalDigitNumber, _
                        Globalization.UnicodeCategory.NonSpacingMark, _
                        Globalization.UnicodeCategory.SpacingCombiningMark, _
                        Globalization.UnicodeCategory.ConnectorPunctuation
                        result.Append(ch)
                    Case Else
                        result.Append(replacementChar)
                End Select
            Next ch

            Dim cleanIdentifier As String = result.ToString()
            If Not CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(cleanIdentifier) Then
                Debug.Fail(String.Format("Failed to clean up identifier '{0}'", cleanIdentifier))
                Throw AppDesCommon.CreateArgumentException("value")
            End If

            Return cleanIdentifier
        End Function

        ''' <summary>
        ''' Try to get the encoding used by a DocData
        ''' </summary>
        ''' <param name="dd"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetEncoding(dd As Shell.Design.Serialization.DocData) As System.Text.Encoding
            ' Try to get the encoding of the textbuffer that we are going to write to...
            Try
                Static GUID_VsBufferEncodingVSTFF As New Guid("{16417F39-A6B7-4c90-89FA-770D2C60440B}")
                Dim oEncoding As Object = Nothing
                Dim userData As TextManager.Interop.IVsUserData = TryCast(dd.Buffer, TextManager.Interop.IVsUserData)
                If userData IsNot Nothing Then
                    VSErrorHandler.ThrowOnFailure(userData.GetData(GUID_VsBufferEncodingVSTFF, oEncoding))
                    If oEncoding IsNot Nothing Then
                        Return System.Text.Encoding.GetEncoding(CInt(oEncoding) And TextManager.Interop.__VSTFF.VSTFF_CPMASK)
                    End If
                End If
            Catch ex As Exception
            End Try
            Return System.Text.Encoding.Default
        End Function

        ''' <summary>
        ''' Returns true is ClickOnce is available for this project
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Function IsClickOnceSupported(hier As IVsHierarchy) As Boolean
            If AppDesCommon.ShellUtil.IsWebProject(hier) Then
                Return False
            End If

            Dim publishableServicePtr As IntPtr
            Try
                Dim cfgs(1) As IVsCfg
                AppDesCommon.ShellUtil.GetConfigProvider(hier).GetCfgs(1, cfgs)
                Dim cgfs2 As IVsProjectCfg2 = TryCast(cfgs(0), IVsProjectCfg2)
                cgfs2.get_CfgType(GetType(IVsPublishableProjectCfg).GUID, publishableServicePtr)
                Dim publishableService As IVsPublishableProjectCfg = DirectCast(Marshal.GetObjectForIUnknown(publishableServicePtr), IVsPublishableProjectCfg)
                Dim publishable(1) As Integer
                Dim ready(1) As Integer
                Dim hr As Integer = publishableService.QueryStartPublish(0, publishable, ready)
                VSErrorHandler.ThrowOnFailure(hr)
                Return CBool(publishable(0))
            Finally
                If Not publishableServicePtr.Equals(0) Then
                    Marshal.Release(publishableServicePtr)
                    publishableServicePtr = New IntPtr(0)
                End If
            End Try
        End Function

    End Class 'DesignUtil

End Namespace


