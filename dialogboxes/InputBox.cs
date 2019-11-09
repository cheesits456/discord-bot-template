using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Forms;


namespace RobvanderWoude
{
	class InputBox
	{
		public const string progver = "1.35";

		#region Global Variables

		public const int defheight = 110;
		public const int deftimeout = 60;
		public const int defwidth = 200;
		public const string defaulttitle = "© 2018 Rob van der Woude";

		public static ConsoleColor bold = ConsoleColor.White;
		public static MaskedTextBox maskedtextbox;
		public static TextBox textbox;
		public static RegexOptions casesensitivity = RegexOptions.None;
		public static bool asciionly = false;
		public static bool bw = false;
		public static bool filtered = true;
		public static bool oddrow = false;
		public static bool ontheflyregexset = false;
		public static bool password = false;
		public static bool timeoutelapsed = false;
		public static bool usemask = false;
		public static bool regexset = false;
		public static bool returnunmasked = false;
		public static string currentinput = String.Empty;
		public static string defanswer = "Default answer";
		public static string ontheflypattern = ".*";
		public static string previousinput = String.Empty;
		public static string regexpattern = ".*";

		#endregion Global Variables


		[STAThread]
		static int Main( string[] args )
		{
			// Based on code by Gorkem Gencay on StackOverflow.com:
			// http://stackoverflow.com/questions/97097/what-is-the-c-sharp-version-of-vb-nets-inputdialog#17546909

			#region Initialize variables

			const string deftitle = "Title";
			const string deftext = "Prompt";

			bool heightset = false;
			bool showpassword = false;
			bool timeoutset = false;
			bool widthset = false;
			string input = string.Empty;
			string mask = String.Empty;
			string showpasswordprompt = "Show password";
			string text = deftext;
			string title = deftitle;
			int height = defheight;
			int timeout = 0;
			int width = defwidth;
			string cancelcaption = "&Cancel";
			string okcaption = "&OK";
			string localizationstring = String.Empty;
			bool localizedcaptionset = false;

			#endregion Initialize variables


			#region Command Line Parsing

			if ( args.Length == 0 )
			{
				return ShowHelp( );
			}

			foreach ( string arg in args )
			{
				if ( arg == "/?" )
				{
					return ShowHelp( );
				}
			}

			text = String.Empty;
			title = String.Empty;
			defanswer = String.Empty;

			foreach ( string arg in args )
			{
				if ( arg[0] == '/' )
				{
					if ( arg.Length == 1 )
					{
						return ShowHelp( );
					}
					else if ( arg.Length == 2 )
					{
						switch ( arg.ToString( ).ToUpper( ) )
						{
							case "/A":
								if ( asciionly )
								{
									return ShowHelp( "Duplicate command line switch /A" );
								}
								asciionly = true;
								break;
							case "/B":
								if ( bw )
								{
									return ShowHelp( "Duplicate command line switch /B" );
								}
								bw = true;
								bold = Console.ForegroundColor;
								break;
							case "/I":
								if ( casesensitivity == RegexOptions.IgnoreCase )
								{
									return ShowHelp( "Duplicate command line switch /I" );
								}
								casesensitivity = RegexOptions.IgnoreCase;
								break;
							case "/L":
								if ( localizedcaptionset )
								{
									return ShowHelp( "Duplicate command line switch /L" );
								}
								localizedcaptionset = true;
								break;
							case "/M":
								return HelpMessage( "mask" );
							case "/N":
								if ( !filtered )
								{
									return ShowHelp( "Duplicate command line switch /N" );
								}
								filtered = false;
								break;
							case "/P":
								if ( password )
								{
									return ShowHelp( "Duplicate command line switch /P" );
								}
								password = true;
								break;
							case "/S":
								if ( showpassword )
								{
									return ShowHelp( "Duplicate command line switch /S" );
								}
								showpassword = true;
								break;
							case "/T":
								if ( timeoutset )
								{
									return ShowHelp( "Duplicate command line switch /T" );
								}
								timeout = deftimeout;
								timeoutset = true;
								break;
							case "/U":
								if ( returnunmasked )
								{
									return ShowHelp( "Duplicate command line switch /U" );
								}
								returnunmasked = true;
								break;
							default:
								return ShowHelp( "Invalid command line switch {0}", arg );
						}
					}
					else if ( arg.Length > 3 && arg[2] == ':' )
					{
						switch ( arg.Substring( 0, 3 ).ToUpper( ) )
						{
							case "/F:":
								if ( ontheflyregexset )
								{
									return ShowHelp( "Duplicate command line switch /F" );
								}
								ontheflypattern = String.Format( "^{0}$", arg.Substring( 3 ) );
								ontheflyregexset = true;
								break;
							case "/H:":
								if ( heightset )
								{
									return ShowHelp( "Duplicate command line switch /H" );
								}
								try
								{
									height = Convert.ToInt32( arg.Substring( 3 ) );
									if ( height < defheight || height > Screen.PrimaryScreen.Bounds.Height )
									{
										return ShowHelp( "Invalid screen height: \"{0}\"\n\tHeight must be an integer between {1} and {2} (screen height)", arg.Substring( 3 ), defheight.ToString( ), Screen.PrimaryScreen.Bounds.Height.ToString( ) );
									}
									heightset = true;
								}
								catch ( FormatException e )
								{
									return ShowHelp( "Invalid height: \"{0}\"\n\t{1}", arg.Substring( 3 ), e.Message );
								}
								break;
							case "/L:":
								if ( localizedcaptionset )
								{
									return ShowHelp( "Duplicate command line switch /L" );
								}
								localizedcaptionset = true;
								localizationstring = arg.Substring( 3 );
								break;
							case "/M:":
								if ( usemask )
								{
									return ShowHelp( "Duplicate command line switch /M" );
								}
								mask = arg.Substring( 3 ).Trim( "\"".ToCharArray( ) );
								if ( String.IsNullOrWhiteSpace( mask ) )
								{
									ShowHelp( "No mask specified with /M" );
									Console.WriteLine( "\n\n" );
									return HelpMessage( "mask" );
								}
								usemask = true;
								break;
							case "/R:":
								if ( regexset )
								{
									return ShowHelp( "Duplicate command line switch /R" );
								}
								regexpattern = arg.Substring( 3 );
								regexset = true;
								break;
							case "/S:":
								if ( showpassword )
								{
									return ShowHelp( "Duplicate command line switch /S" );
								}
								showpassword = true;
								showpasswordprompt = arg.Substring( 3 );
								break;
							case "/T:":
								if ( timeoutset )
								{
									return ShowHelp( "Duplicate command line switch /T" );
								}
								try
								{
									timeout = Convert.ToInt32( arg.Substring( 3 ) ) * 1000;
									if ( timeout < 1000 )
									{
										return ShowHelp( "Invalid timeout: \"{0}\"\n\tTimeout value must be a positive integer, at least 1.", arg.Substring( 3 ) );
									}
									timeoutset = true;
								}
								catch ( FormatException e )
								{
									return ShowHelp( "Invalid timeout: \"{0}\"\n\t{1}", arg.Substring( 3 ), e.Message );
								}
								break;
							case "/W:":
								if ( widthset )
								{
									return ShowHelp( "Duplicate command line switch /W" );
								}
								try
								{
									width = Convert.ToInt32( arg.Substring( 3 ) );
									if ( width < defwidth || width > Screen.PrimaryScreen.Bounds.Width )
									{
										return ShowHelp( "Invalid screen width: \"{0}\"\n\tWidth must be an integer between {1} and {2} (screen width)", arg.Substring( 3 ), defwidth.ToString( ), Screen.PrimaryScreen.Bounds.Width.ToString( ) );
									}
									widthset = true;
								}
								catch ( FormatException e )
								{
									return ShowHelp( "Invalid width: \"{0}\"\n\t{1}", arg.Substring( 3 ), e.Message );
								}
								break;
							default:
								return ShowHelp( "Invalid command line switch \"{0}\"", arg );
						}
					}
					else
					{
						return ShowHelp( "Invalid command line argument \"{0}\"", arg );
					}
				}
				else
				{
					if ( String.IsNullOrWhiteSpace( text ) )
					{
						text = arg;
					}
					else if ( String.IsNullOrWhiteSpace( title ) )
					{
						title = arg;
					}
					else if ( String.IsNullOrWhiteSpace( defanswer ) )
					{
						defanswer = arg;
					}
					else
					{
						return ShowHelp( "Invalid command line argument \"{0}\"", arg );
					}
				}
			}

			// Default title if none specified
			if ( String.IsNullOrWhiteSpace( title ) )
			{
				title = defaulttitle;
			}

			// "Bold" color depends on /BW
			if ( bw )
			{
				bold = Console.ForegroundColor;
			}
			else
			{
				bold = ConsoleColor.White;
			}

			// Switch /A requires /M
			if ( asciionly && !usemask )
			{
				return ShowHelp( "Command line switch /A (ASCII only) can only be used together with /M" );
			}

			// Switch /S implies /P
			if ( showpassword )
			{
				password = true;
			}

			// Set timer if /T:timeout was specified
			if ( timeoutset )
			{
				System.Timers.Timer timer = new System.Timers.Timer( );
				timer.Elapsed += new ElapsedEventHandler( timer_Elapsed );
				timer.Interval = timeout;
				timer.Start( );
			}

			// For /S (Show password checkbox) add 25 px to window height unless height is specified
			if ( showpassword && !heightset )
			{
				height += 25;
			}

			#endregion Command Line Parsing


			#region Set Localized Captions

			if ( localizedcaptionset )
			{
				cancelcaption = Load( "user32.dll", 801, cancelcaption );
				okcaption = Load( "user32.dll", 800, okcaption );
				if ( !String.IsNullOrWhiteSpace( localizationstring ) )
				{
					string pattern = @"^((OK|Cancel)=[^;\""]*;)*((OK|Cancel)=[^;\""]*);?$";
					Regex regex = new Regex( pattern, RegexOptions.IgnoreCase );
					if ( regex.IsMatch( localizationstring ) )
					{
						string[] locstrings = localizationstring.Split( ";".ToCharArray( ) );
						foreach ( string locstring in locstrings )
						{
							string key = locstring.Substring( 0, locstring.IndexOf( '=' ) );
							string val = locstring.Substring( Math.Min( locstring.IndexOf( '=' ) + 1, locstring.Length - 1 ) );
							if ( !String.IsNullOrWhiteSpace( val ) )
							{
								switch ( key.ToUpper( ) )
								{
									case "OK":
										okcaption = val;
										break;
									case "CANCEL":
										cancelcaption = val;
										break;
									default:
										return ShowHelp( "Invalid localization key \"{0}\"", key );
								}
							}
						}
					}
					else
					{
						return ShowHelp( "Invalid localization string:\n\t{0}", localizationstring );
					}
				}
			}

			#endregion Set Localized Captions


			#region Define Form

			Size size = new Size( width, height );
			Form inputBox = new Form( );

			inputBox.FormBorderStyle = FormBorderStyle.FixedDialog;
			inputBox.MaximizeBox = false;
			inputBox.MinimizeBox = false;
			inputBox.StartPosition = FormStartPosition.CenterParent;
			inputBox.ClientSize = size;
			inputBox.Text = title;

			Label labelPrompt = new Label( );
			labelPrompt.Size = new Size( width - 20, height - 90 );
			labelPrompt.Location = new Point( 10, 10 );
			labelPrompt.Text = text.Replace( "\\n", "\n" );
			inputBox.Controls.Add( labelPrompt );

			textbox = new TextBox( );
			textbox.Size = new Size( width - 20, 25 );
			if ( showpassword )
			{
				textbox.Location = new Point( 10, height - 100 );
			}
			else
			{
				textbox.Location = new Point( 10, height - 75 );
			}
			if ( password )
			{
				textbox.PasswordChar = '*';
				if ( showpassword )
				{
					// Insert a checkbox with label "Show password" 25 px below the textbox
					CheckBox checkbox = new CheckBox( );
					checkbox.Checked = false;
					checkbox.Location = new Point( 11, textbox.Location.Y + 25 );
					checkbox.Width = inputBox.Width - 22;
					checkbox.Click += new EventHandler( checkbox_Click );
					checkbox.Text = showpasswordprompt;
					inputBox.Controls.Add( checkbox );
				}
			}
			else
			{
				textbox.Text = defanswer;
			}

			maskedtextbox = new MaskedTextBox( );
			maskedtextbox.Mask = mask;
			maskedtextbox.Location = textbox.Location;
			maskedtextbox.PasswordChar = textbox.PasswordChar;
			maskedtextbox.Text = textbox.Text;
			maskedtextbox.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals; // return only the raw input
			maskedtextbox.Size = textbox.Size;
			maskedtextbox.AsciiOnly = asciionly;

			if ( usemask )
			{
				maskedtextbox.KeyUp += new KeyEventHandler( maskedtextbox_KeyUp );
				inputBox.Controls.Add( maskedtextbox );
			}
			else
			{
				textbox.KeyUp += new KeyEventHandler( textbox_KeyUp );
				inputBox.Controls.Add( textbox );
			}

			Button okButton = new Button( );
			okButton.DialogResult = DialogResult.OK;
			okButton.Name = "okButton";
			okButton.Size = new Size( 80, 25 );
			okButton.Text = okcaption;
			okButton.Location = new Point( width / 2 - 10 - 80, height - 40 );
			inputBox.Controls.Add( okButton );

			Button cancelButton = new Button( );
			cancelButton.DialogResult = DialogResult.Cancel;
			cancelButton.Name = "cancelButton";
			cancelButton.Size = new Size( 80, 25 );
			cancelButton.Text = cancelcaption;
			cancelButton.Location = new Point( width / 2 + 10, height - 40 );
			inputBox.Controls.Add( cancelButton );

			inputBox.AcceptButton = okButton;  // OK on Enter
			inputBox.CancelButton = cancelButton; // Cancel on Esc
			inputBox.Activate( );
			inputBox.BringToFront( );
			inputBox.Focus( );

			if ( usemask )
			{
				maskedtextbox.BringToFront( ); // Bug workaround
				maskedtextbox.Select( 0, 0 ); // Move cursor to begin
				maskedtextbox.Focus( );
			}
			else
			{
				textbox.BringToFront( ); // Bug workaround
				textbox.Select( 0, 0 ); // Move cursor to begin
				textbox.Focus( );
			}

			#endregion Define Form


			#region Show Dialog and Return Result

			DialogResult result = inputBox.ShowDialog( );
			if ( result == DialogResult.OK )
			{
				int rc = ValidateAndShowResult( );
				return rc;
			}
			else
			{
				if ( timeoutelapsed )
				{
					ValidateAndShowResult( );
					return 3;
				}
				else
				{
					return 2;
				}
			}

			#endregion Show Dialog and Return Result
		}


		#region Event Handlers

		public static void checkbox_Click( object sender, System.EventArgs e )
		{
			// Toggle between hidden and normal text
			if ( usemask )
			{
				if ( maskedtextbox.PasswordChar == '*' )
				{
					maskedtextbox.PasswordChar = '\0';
				}
				else
				{
					maskedtextbox.PasswordChar = '*';
				}
			}
			else
			{
				if ( textbox.PasswordChar == '*' )
				{
					textbox.PasswordChar = '\0';
				}
				else
				{
					textbox.PasswordChar = '*';
				}
			}
		}


		private static void maskedtextbox_KeyUp( object sender, KeyEventArgs e )
		{
			maskedtextbox.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
			currentinput = maskedtextbox.Text;
			if ( Regex.IsMatch( currentinput, ontheflypattern, casesensitivity ) )
			{
				previousinput = currentinput;
			}
			else
			{
				currentinput = previousinput;
			}
			if ( maskedtextbox.Text != currentinput )
			{
				maskedtextbox.Text = currentinput;
				maskedtextbox.TextMaskFormat = MaskFormat.IncludeLiterals;
				if ( currentinput.Length > 0 )
				{
					maskedtextbox.SelectionStart = maskedtextbox.Text.LastIndexOf( currentinput.Last<char>( ) ) + 1;
				}
				else
				{
					maskedtextbox.SelectionStart = 0;
				}
				maskedtextbox.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
			}
		}


		private static void textbox_KeyUp( object sender, KeyEventArgs e )
		{
			currentinput = textbox.Text;
			if ( Regex.IsMatch( currentinput, ontheflypattern, casesensitivity ) )
			{
				previousinput = currentinput;
			}
			else
			{
				currentinput = previousinput;
			}
			if ( textbox.Text != currentinput )
			{
				textbox.Text = currentinput;
				textbox.SelectionStart = currentinput.Length;
			}
		}


		public static void timer_Elapsed( object sender, System.EventArgs e )
		{
			timeoutelapsed = true;
			Process.GetCurrentProcess( ).CloseMainWindow( );
		}

		#endregion Event Handlers


		public static int HelpMessage( string subject )
		{
			switch ( subject.ToLower( ) )
			{
				case "mask":
					int col1perc = 13;
					Console.Error.Write( "Help for command line switch " );
					Console.ForegroundColor = bold;
					Console.Error.WriteLine( "/M:mask" );
					Console.ResetColor( );
					Console.Error.WriteLine( );
					Console.Error.Write( "The " );
					Console.ForegroundColor = bold;
					Console.Error.Write( "mask" );
					Console.ResetColor( );
					Console.Error.WriteLine( " \"language\" is based on the Masked Edit control in Visual Basic 6.0:" );
					if ( !bw )
					{
						Console.ForegroundColor = ConsoleColor.DarkGray;
					}
					string url1 = "http://msdn.microsoft.com/en-us/library/";
					string url2 = "system.windows.forms.maskedtextbox.mask.aspx#remarksToggle";
					if ( url1.Length + url2.Length > Console.WindowWidth )
					{
						Console.Error.WriteLine( url1 );
						Console.Error.WriteLine( url2 );
					}
					else
					{
						Console.Error.WriteLine( url1 + url2 );
					}
					Console.ResetColor( );
					Console.Error.WriteLine( );
					WriteTableRow( "Masking element", "Description", col1perc, true, true );
					WriteTableRow( "0", "Digit, required. This element will accept any single digit between 0 and 9.", col1perc );
					WriteTableRow( "9", "Digit or space, optional.", col1perc );
					WriteTableRow( "#", "Digit or space, optional. If this position is blank in the mask, it will be rendered as a space in the Text property. Plus (+) and minus (-) signs are allowed.", col1perc );
					WriteTableRow( "L", "Letter, required. Restricts input to the ASCII letters a-z and A-Z. This mask element is equivalent to [a-zA-Z] in regular expressions.", col1perc );
					WriteTableRow( "?", "Letter, optional. Restricts input to the ASCII letters a-z and A-Z. This mask element is equivalent to [a-zA-Z]? in regular expressions.", col1perc );
					WriteTableRow( "&", "Character, required. Any non-control character. If ASCII only is set (/A), this element behaves like the \"A\" element.", col1perc );
					WriteTableRow( "C", "Character, optional. Any non-control character. If ASCII only is set (/A), this element behaves like the \"a\" element.", col1perc );
					WriteTableRow( "A", "Alphanumeric, required. If ASCII only is set (/A), the only characters it will accept are the ASCII letters a-z and A-Z and numbers. This mask element behaves like the \"&\" element.", col1perc );
					WriteTableRow( "a", "Alphanumeric, optional. If ASCII only is set (/A), the only characters it will accept are the ASCII letters a-z and A-Z and numbers. This mask element behaves like the \"C\" element.", col1perc );
					WriteTableRow( ".", "Decimal placeholder.", col1perc );
					WriteTableRow( ",", "Thousands placeholder.", col1perc );
					WriteTableRow( ":", "Time separator.", col1perc );
					WriteTableRow( "/", "Date separator.", col1perc );
					WriteTableRow( "$", "Currency symbol.", col1perc );
					WriteTableRow( "<", "Shift down. Converts all characters that follow to lowercase.", col1perc );
					WriteTableRow( ">", "Shift up. Converts all characters that follow to uppercase.", col1perc );
					WriteTableRow( "|", "Disable a previous shift up or shift down.", col1perc );
					WriteTableRow( @"\", "Escape. Escapes a mask character, turning it into a literal. \"\\\\\" is the escape sequence for a backslash.", col1perc );
					WriteTableRow( "All other characters", "Literals. All non-mask elements will appear as themselves within MaskedTextBox. Literals always occupy a static position in the mask at run time, and cannot be moved or deleted by the user.", col1perc );
					break;
				default:
					return ShowHelp( );
			}
			return 1;
		}

		
		public static int ShowHelp( params string[] errmsg )
		{
			/*
			InputBox,  Version 1.34
			Prompt for input (GUI)

			Usage:   INPUTBOX  [ "prompt"  [ "title"  [ "default" ] ] ] [ options ]

			Where:   "prompt"    is the text above the input field (use \n for new line)
			         "title"     is the caption in the title bar
			         "default"   is the default answer shown in the input field

			Options: /A          accepts ASCII characters only (requires /M)
			         /B          use standard Black and white in console, no highlighting
			         /F:regex    use regex to filter input on-the-Fly (see Notes)
			         /H:height   sets the Height of the input box
			                     (default: 110; minimum: 110; maximum: screen height)
			         /I          regular expressions are case Insensitive
			                     (default: regular expressions are case sensitive)
			         /L[:string] use Localized or custom captions (see Notes)
			         /M:mask     accept input only if it matches mask (template)
			         /N          Not filtered, only doublequotes are removed from input
			                     (default: remove & < > | ")
			         /P          hides (masks) the input text (for Passwords)
			         /R:regex    accept input only if it matches Regular expression regex
			         /S[:text]   inserts a checkbox "Show password" (or specified text)
			         /T[:sec]    sets the optional Timeout in seconds (default: 60)
			         /U          return Unmasked input, without literals (requires /M)
			                     (default: include literals in result)
			         /W:width    sets the Width of the input box
			                     (default: 200; minimum: 200; maximum: screen width)

			Example: prompt for password
			InputBox.exe "Enter your password:" "Login" /S

			Example: fixed length hexadecimal input (enter as a single command line)
			InputBox.exe "Enter a MAC address:" "MAC Address" "0022446688AACCEE"
			             /M:">CC\:CC\:CC\:CC\:CC\:CC" /R:"[\dA-F]{16}"
			             /F:"[\dA-F]{1,16}" /U /I

			Notes:   For hidden input (/P and/or /S), "default" will be ignored.
			         With /F, regex must test the unmasked input (without literals), e.g.
			         /M:"CC:CC:CC:CC:CC:CC:CC:CC" /F:"[\dA-F]{0,16} /I" for MAC address.
			         With /R, regex is used to test input after OK is clicked;
			         with /F, regex is used to test input each time the input
			         changes, so regex must be able to cope with partial input;
			         e.g. /F:"[\dA-F]{0,16}" is OK, but /F:"[\dA-F]{16}" will fail.
			         Be careful with /N, use doublequotes for the "captured" result,
			         or redirect the result to a (temporary) file.
			         Show password (/S) implies hiding the input text (/P).
			         Use /M (without mask) to show detailed help on the mask language.
			         Use /L for Localized "OK" and "Cancel" button captions.
			         Custom captions require a string like /L:"OK=caption;Cancel=caption"
			         (button=caption pairs separated by semicolons, each button optional).
			         Text from input is written to Standard Out only if "OK" is clicked.
			         Return code is 0 for "OK", 1 for (command line) errors, 2 for
			         "Cancel", 3 on timeout, 4 if no regex or mask match.

			Credits: On-the-fly form based on code by Gorkem Gencay on StackOverflow:
			         http://stackoverflow.com/questions/97097#17546909
			         Code to retrieve localized button captions by Martin Stoeckli:
			         http://martinstoeckli.ch/csharp/csharp.html#windows_text_resources

			Written by Rob van der Woude
			http://www.robvanderwoude.com
			*/

			if ( errmsg.Length > 0 )
			{
				List<string> errargs = new List<string>( errmsg );
				errargs.RemoveAt( 0 );
				Console.Error.WriteLine( );
				if ( !bw )
				{
					Console.ForegroundColor = ConsoleColor.Red;
				}
				Console.Error.Write( "ERROR:\t" );
				Console.ForegroundColor = bold;
				Console.Error.WriteLine( errmsg[0], errargs.ToArray( ) );
				Console.ResetColor( );
			}

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "InputBox,  Version {0}", progver );

			Console.Error.WriteLine( "Prompt for input (GUI)" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Usage:   " );
			Console.ForegroundColor = bold;
			Console.Error.WriteLine( "INPUTBOX  [ \"prompt\"  [ \"title\"  [ \"default\" ] ] ] [ options ]" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Where:   " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "\"prompt\"" );
			Console.ResetColor( );
			Console.Error.WriteLine( "    is the text above the input field (use \\n for new line)" );

			Console.ForegroundColor = bold;
			Console.Error.Write( "         \"title\"" );
			Console.ResetColor( );
			Console.Error.WriteLine( "     is the caption in the title bar" );

			Console.ForegroundColor = bold;
			Console.Error.Write( "         \"default\"" );
			Console.ResetColor( );
			Console.Error.WriteLine( "   is the default answer shown in the input field" );

			Console.Error.WriteLine( );

			Console.Error.Write( "Options: " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "/A" );
			Console.ResetColor( );
			Console.Error.Write( "          accepts " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "A" );
			Console.ResetColor( );
			Console.Error.Write( "SCII characters only (requires " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "/M" );
			Console.ResetColor( );
			Console.Error.WriteLine( ")" );

			Console.ForegroundColor = bold;
			Console.Error.Write( "         /B" );
			Console.ResetColor( );
			Console.Error.Write( "          use standard " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "B" );
			Console.ResetColor( );
			Console.Error.WriteLine( "lack and white in console, no highlighting" );

			Console.ForegroundColor = bold;
			Console.Error.Write( "         /F:regex" );
			Console.ResetColor( );
			Console.Error.Write( "    use " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "regex" );
			Console.ResetColor( );
			Console.Error.Write( " to filter input on-the-" );
			Console.ForegroundColor = bold;
			Console.Error.Write( "F" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ly (see Notes)" );

			Console.ForegroundColor = bold;
			Console.Error.Write( "         /H:height" );
			Console.ResetColor( );
			Console.Error.Write( "   sets the " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "H" );
			Console.ResetColor( );
			Console.Error.WriteLine( "eight of the input box" );

			Console.Error.WriteLine( "                     (default: {0}; minimum: {0}; maximum: screen height)", defheight );

			Console.ForegroundColor = bold;
			Console.Error.Write( "         /I" );
			Console.ResetColor( );
			Console.Error.Write( "          regular expressions are case " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "I" );
			Console.ResetColor( );
			Console.Error.WriteLine( "nsensitive" );

			Console.Error.WriteLine( "                     (default: regular expressions are case sensitive)" );

			Console.ForegroundColor = bold;
			Console.Error.Write( "         /L[:string]" );
			Console.ResetColor( );
			Console.Error.Write( " use " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "L" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ocalized or custom captions (see Notes)" );

			Console.ForegroundColor = bold;
			Console.Error.Write( "         /M:mask" );
			Console.ResetColor( );
			Console.Error.Write( "     accept input only if it matches " );
			Console.ForegroundColor = bold;
			Console.Error.WriteLine( "mask" );
			Console.ResetColor( );

			Console.ForegroundColor = bold;
			Console.Error.Write( "         /N          N" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ot filtered, only doublequotes are removed from input" );

			Console.Error.Write( "                     (default: remove " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "& < > | \"" );
			Console.ResetColor( );
			Console.Error.WriteLine( ")" );

			Console.ForegroundColor = bold;
			Console.Error.Write( "         /P" );
			Console.ResetColor( );
			Console.Error.Write( "          hides (masks) the input text (for " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "P" );
			Console.ResetColor( );
			Console.Error.WriteLine( "asswords)" );

			Console.ForegroundColor = bold;
			Console.Error.Write( "         /R:regex" );
			Console.ResetColor( );
			Console.Error.Write( "    accept input only if it matches " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "R" );
			Console.ResetColor( );
			Console.Error.Write( "egular expression " );
			Console.ForegroundColor = bold;
			Console.Error.WriteLine( "regex" );
			Console.ResetColor( );

			Console.ForegroundColor = bold;
			Console.Error.Write( "         /S[:text]" );
			Console.ResetColor( );
			Console.Error.Write( "   inserts a checkbox \"" );
			Console.ForegroundColor = bold;
			Console.Error.Write( "S" );
			Console.ResetColor( );
			Console.Error.Write( "how password\" (or specified " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "text" );
			Console.ResetColor( );
			Console.Error.WriteLine( ")" );

			Console.ForegroundColor = bold;
			Console.Error.Write( "         /T[:sec]" );
			Console.ResetColor( );
			Console.Error.Write( "    sets the optional " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "T" );
			Console.ResetColor( );
			Console.Error.Write( "imeout in " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "sec" );
			Console.ResetColor( );
			Console.Error.WriteLine( "onds (default: {0})", deftimeout );

			Console.ForegroundColor = bold;
			Console.Error.Write( "         /U" );
			Console.ResetColor( );
			Console.Error.Write( "          return " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "U" );
			Console.ResetColor( );
			Console.Error.Write( "nmasked input, without literals (requires " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "/M" );
			Console.ResetColor( );
			Console.Error.WriteLine( ")" );

			Console.Error.WriteLine( "                     (default: include literals in result)" );

			Console.ForegroundColor = bold;
			Console.Error.Write( "         /W:width" );
			Console.ResetColor( );
			Console.Error.Write( "    sets the " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "W" );
			Console.ResetColor( );
			Console.Error.WriteLine( "idth of the input box" );

			Console.Error.WriteLine( "                     (default: {0}; minimum: {0}; maximum: screen width)", defwidth );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Example: prompt for password" );
			Console.ForegroundColor = bold;

			Console.Error.WriteLine( "InputBox.exe \"Enter your password:\" \"Login\" /S" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Example: fixed length hexadecimal input (enter as a single command line)" );

			Console.ForegroundColor = bold;
			Console.Error.WriteLine( "InputBox.exe \"Enter a MAC address:\" \"MAC Address\" \"0022446688AACCEE\"" );

			Console.Error.WriteLine( "             /M:\">CC\\:CC\\:CC\\:CC\\:CC\\:CC\\:CC\\:CC\" /R:\"[\\dA-F]{16}\"" );

			Console.Error.WriteLine( "             /F:\"[\\dA-F]{0,16}\" /U /I" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.Write( "Notes:   For hidden input (" );
			Console.ForegroundColor = bold;
			Console.Error.Write( "/P" );
			Console.ResetColor( );
			Console.Error.Write( " and/or " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "/S), " );
			Console.ResetColor( );
			Console.Error.Write( "), " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "\"default\"" );
			Console.ResetColor( );
			Console.Error.WriteLine( " will be ignored." );

			Console.Error.Write( "         With " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "/F" );
			Console.ResetColor( );
			Console.Error.Write( ", " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "regex" );
			Console.ResetColor( );
			Console.Error.Write( " must test the " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "unmasked" );
			Console.ResetColor( );
			Console.Error.WriteLine( " input (without literals), e.g." );

			Console.ForegroundColor = bold;
			Console.Error.Write( "         /M:\"CC:CC:CC:CC:CC:CC:CC:CC\" /F:\"[\\dA-F]{0,16}\" /I" );
			Console.ResetColor( );
			Console.Error.WriteLine( " for MAC address." );

			Console.Error.Write( "         With " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "/R" );
			Console.ResetColor( );
			Console.Error.Write( ", " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "regex" );
			Console.ResetColor( );
			Console.Error.WriteLine( " is used to test input after OK is clicked;" );

			Console.Error.Write( "         with " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "/F" );
			Console.ResetColor( );
			Console.Error.Write( ", " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "regex" );
			Console.ResetColor( );
			Console.Error.WriteLine( " is used to test input each time the input" );

			Console.Error.Write( "         changes, so " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "regex" );
			Console.ResetColor( );
			Console.Error.WriteLine( " must be able to cope with partial input;" );

			Console.Error.Write( "         e.g. " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "/F:\"[\\dA-F]{0,16}\"" );
			Console.ResetColor( );
			Console.Error.Write( " is OK, but " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "/F:\"[\\dA-F]{16}\"" );
			Console.ResetColor( );
			Console.Error.WriteLine( " will fail." );

			Console.Error.Write( "         Be careful with " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "/N" );
			Console.ResetColor( );
			Console.Error.WriteLine( ", use doublequotes for the \"captured\" result," );

			Console.Error.WriteLine( "         or redirect the result to a (temporary) file." );

			Console.Error.Write( "         Show password (" );
			Console.ForegroundColor = bold;
			Console.Error.Write( "/S" );
			Console.ResetColor( );
			Console.Error.Write( ") implies hiding the input text (" );
			Console.ForegroundColor = bold;
			Console.Error.Write( "/P" );
			Console.ResetColor( );
			Console.Error.WriteLine( ")." );

			Console.Error.Write( "         Use " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "/M" );
			Console.ResetColor( );
			Console.Error.Write( " (without " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "mask" );
			Console.ResetColor( );
			Console.Error.Write( ") to show detailed help on the " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "mask" );
			Console.ResetColor( );
			Console.Error.WriteLine( " language." );

			Console.Error.Write( "         Use " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "/L" );
			Console.ResetColor( );
			Console.Error.Write( " for " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "L" );
			Console.ResetColor( );
			Console.Error.WriteLine( "ocalized \"OK\" and \"Cancel\" button captions." );

			Console.Error.Write( "         Custom captions require a " );
			Console.ForegroundColor = bold;
			Console.Error.Write( "string" );
			Console.ResetColor( );
			Console.Error.Write( " like " );
			Console.ForegroundColor = bold;
			Console.Error.WriteLine( "/L:\"OK=caption;Cancel=caption\"" );
			Console.ResetColor( );

			Console.Error.WriteLine( "         (button=caption pairs separated by semicolons, each button optional)" );

			Console.Error.WriteLine( "         Text from input is written to Standard Output only if \"OK\" is clicked." );

			Console.Error.WriteLine( "         Return code is 0 for \"OK\", 1 for (command line) errors, 2 for" );

			Console.Error.WriteLine( "         \"Cancel\", 3 on timeout, 4 if no regex or mask match." );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Credits: On-the-fly form based on code by Gorkem Gencay on StackOverflow:" );

			if ( !bw )
			{
				Console.ForegroundColor = ConsoleColor.DarkGray;
			}
			Console.Error.WriteLine( "         http://stackoverflow.com/questions/97097#17546909" );
			Console.ResetColor( );

			Console.Error.WriteLine( "         Code to retrieve localized button captions by Martin Stoeckli:" );

			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Error.WriteLine( "         http://martinstoeckli.ch/csharp/csharp.html#windows_text_resources" );
			Console.ResetColor( );

			Console.Error.WriteLine( );

			Console.Error.WriteLine( "Written by Rob van der Woude" );

			Console.Error.WriteLine( "http://www.robvanderwoude.com" );

			return 1;
		}


		public static int ValidateAndShowResult( )
		{
			string input = String.Empty;
			// Read input from MaskedTextBox or TextBox
			if ( usemask )
			{
				if ( returnunmasked )
				{
					maskedtextbox.TextMaskFormat = MaskFormat.ExcludePromptAndLiterals;
				}
				else
				{
					maskedtextbox.TextMaskFormat = MaskFormat.IncludeLiterals;
				}
				input = maskedtextbox.Text;
				// Check if input complies with mask
				if ( !maskedtextbox.MaskCompleted )
				{
					return 4;
				}
			}
			else
			{
				input = textbox.Text;
			}

			// Check if input complies with regex
			if ( regexset && Regex.IsMatch( input, regexpattern, casesensitivity ) )
			{
				return 4;
			}

			// Remove ampersands and redirection symbols unless /N switch was used
			if ( filtered )
			{
				input = Regex.Replace( input, @"[&<>|]", String.Empty );
			}

			// Remove doublequotes from output
			input = input.Replace( "\"", "" );
			Console.WriteLine( input );
			return 0;
		}


		public static void WriteTableRow( string col1text, string col2text, int col1percentage, bool col1bold = true, bool col2bold = false )
		{
			// Wrap text to fit in 2 columns
			oddrow = !oddrow;
			int windowwidth = Console.WindowWidth;
			int col1width = Convert.ToInt32( windowwidth * col1percentage / 100 );
			int col2width = windowwidth - col1width - 5; // Column separator = 4, subtract 1 extra to prevent automatic line wrap
			List<string> col1lines = new List<string>( );
			List<string> col2lines = new List<string>( );
			// Column 1
			if ( col1text.Length > col1width )
			{
				Regex regex = new Regex( @".{1," + col1width + @"}(?=\s|$)" );
				if ( regex.IsMatch( col1text ) )
				{
					MatchCollection matches = regex.Matches( col1text );
					foreach ( Match match in matches )
					{
						col1lines.Add( match.ToString( ).Trim( ) );
					}
				}
				else
				{
					while ( col1text.Length > 0 )
					{
						col1lines.Add( col1text.Trim( ).Substring( 0, Math.Min( col1width, col1text.Length ) ) );
						col1text = col1text.Substring( Math.Min( col1width, col1text.Length ) ).Trim( );
					}
				}
			}
			else
			{
				col1lines.Add( col1text.Trim( ) );
			}
			// Column 2
			if ( col2text.Length > col2width )
			{
				Regex regex = new Regex( @".{1," + col2width + @"}(?=\s|$)" );
				if ( regex.IsMatch( col2text ) )
				{
					MatchCollection matches = regex.Matches( col2text );
					foreach ( Match match in matches )
					{
						col2lines.Add( match.ToString( ).Trim( ) );
					}
				}
				else
				{
					while ( col2text.Length > 0 )
					{
						col2lines.Add( col2text.Trim( ).Substring( 0, Math.Min( col2width, col2text.Length ) ) );
						col2text = col2text.Substring( Math.Min( col2width, col2text.Length ) ).Trim( );
					}
				}
			}
			else
			{
				col2lines.Add( col2text.Trim( ) );
			}
			for ( int i = 0; i < Math.Max( col1lines.Count, col2lines.Count ); i++ )
			{
				if ( oddrow && !bw )
				{
					Console.BackgroundColor = ConsoleColor.DarkGray;
				}
				if ( col1bold || oddrow )
				{
					Console.ForegroundColor = bold;
				}
				Console.Write( "{0,-" + col1width + "}    ", ( i < col1lines.Count ? col1lines[i] : String.Empty ) );
				Console.ResetColor( );
				if ( oddrow && !bw )
				{
					Console.BackgroundColor = ConsoleColor.DarkGray;
				}
				if ( col2bold || oddrow )
				{
					Console.ForegroundColor = bold;
				}
				Console.WriteLine( "{0,-" + col2width + "}", ( i < col2lines.Count ? col2lines[i] : String.Empty ) );
				Console.ResetColor( );
			}
		}


		#region Get Localized Captions

		// Code to retrieve localized captions by Martin Stoeckli
		// http://martinstoeckli.ch/csharp/csharp.html#windows_text_resources

		/// <summary>
		/// Searches for a text resource in a Windows library.
		/// Sometimes, using the existing Windows resources, you can make your code
		/// language independent and you don't have to care about translation problems.
		/// </summary>
		/// <example>
		///   btnCancel.Text = Load("user32.dll", 801, "Cancel");
		///   btnYes.Text = Load("user32.dll", 805, "Yes");
		/// </example>
		/// <param name="libraryName">Name of the windows library like "user32.dll"
		/// or "shell32.dll"</param>
		/// <param name="ident">Id of the string resource.</param>
		/// <param name="defaultText">Return this text, if the resource string could
		/// not be found.</param>
		/// <returns>Requested string if the resource was found,
		/// otherwise the <paramref name="defaultText"/></returns>
		public static string Load( string libraryName, UInt32 ident, string defaultText )
		{
			IntPtr libraryHandle = GetModuleHandle( libraryName );
			if ( libraryHandle != IntPtr.Zero )
			{
				StringBuilder sb = new StringBuilder( 1024 );
				int size = LoadString( libraryHandle, ident, sb, 1024 );
				if ( size > 0 )
					return sb.ToString( );
			}
			return defaultText;
		}

		[DllImport( "kernel32.dll", CharSet = CharSet.Auto )]
		private static extern IntPtr GetModuleHandle( string lpModuleName );

		[DllImport( "user32.dll", CharSet = CharSet.Auto )]
		private static extern int LoadString( IntPtr hInstance, UInt32 uID, StringBuilder lpBuffer, Int32 nBufferMax );

		#endregion Get Localized Captions
	}
}
