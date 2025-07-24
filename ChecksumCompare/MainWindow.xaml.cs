using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows;
using System.Security.Cryptography;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace ChecksumCompare;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow {
	// Form Initialization

	/// <summary>
	/// Initialize MainWindow
	/// </summary>
	public MainWindow() {
		InitializeComponent();
	}

	// Form Methods

	/// <summary>
	/// Opens dialog to select file to grab hash from left button
	/// </summary>
	private async void LeftButton_Click(object sender, RoutedEventArgs e) {
		try {
			// New openFileDialog for user to select a file
			var openFileDialog = new OpenFileDialog {
				Filter = "All files (*.*)|*.*" // Allow all files
			};

			if (openFileDialog.ShowDialog() != true) return; // User cancelled operation
			string filePath = openFileDialog.FileName; // Grab filepath from selected file
			string fileHash = await GrabHashFromFile(filePath); // Grab hash from file

			LeftTextBox.Text = fileHash; // Output hash to LeftTextBox
		} catch (Exception ex) {
			Debug.WriteLine($"Caught exception: {ex.Message}"); // Output exception message
			MessageBox.Show(ex.Message, "Error grabbing hash", MessageBoxButton.OK, MessageBoxImage.Error); // Output error to user
		}
	}
	/// <summary>
	/// Updates emoji when a textbox's content has been changed
	/// </summary>
	private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
		UpdateEmoji();
	}
	/// <summary>
	/// Opens dialog to select file to grab hash when right button is clicked
	/// </summary>
	private async void RightButton_Click(object sender, RoutedEventArgs e) {
		try {
			// New openFileDialog for user to select a file
			var openFileDialog = new OpenFileDialog {
				Filter = "All files (*.*)|*.*" // Allow all files
			};

			if (openFileDialog.ShowDialog() != true) return; // User cancelled operation
			string filePath = openFileDialog.FileName; // Grab filepath from selected file
			string fileHash = await GrabHashFromFile(filePath); // Grab hash from file

			RightTextBox.Text = fileHash; // Output hash to RightTextBox
		} catch (Exception ex) {
			Debug.WriteLine($"Caught exception: {ex.Message}"); // Output exception message
			MessageBox.Show(ex.Message, "Error grabbing hash", MessageBoxButton.OK, MessageBoxImage.Error); // Output error to user
		}
	}
	/// <summary>
	/// Called once when a file is dragged over a text box form control
	/// </summary>
	private void TextBox_OnDragEnter(object sender, DragEventArgs dragEventArgs) {
		ChangeCursorOnDrag(dragEventArgs);
	}
	/// <summary>
	/// Called when a file is dropped over a text box form control
	/// </summary>
	private async void TextBox_OnDrop(object sender, DragEventArgs dragEventArgs) {
		try {
			// Ensure a textbox fired event
			if (sender is not TextBox textBoxControl) return;
			textBoxControl.Text = await GrabHashFromFile(dragEventArgs);
			ResetTextBoxStyle(textBoxControl);
		} catch (Exception ex) {
			Debug.WriteLine($"Caught exception: {ex.Message}"); // Output exception message
			MessageBox.Show(ex.Message, "Error grabbing hash", MessageBoxButton.OK, MessageBoxImage.Error); // Output error to user
		}
	}
	/// <summary>
	/// Called when a file is dragged away from a text box form control
	/// </summary>
	private void TextBox_OnPreviewDragLeave(object sender, DragEventArgs dragEventArgs) {
		// Ensure a text box fired event
		if (sender is not TextBox textBoxControl) return;
		ResetTextBoxStyle(textBoxControl);
	}
	/// <summary>
	/// Called while a file is dragged over a textbox form control
	/// </summary>
	private void TextBox_OnPreviewDragOver(object sender, DragEventArgs dragEventArgs) {
		// Ensure a textbox fired event
		if (sender is not TextBox textBoxControl) return;
		HighlightTextBox(textBoxControl, Brushes.DeepSkyBlue, 2);
		dragEventArgs.Handled = true;
	}

	// Methods

	/// <summary>
	/// Changes mouse cursor depending on drag event. Change to copy cursor when file is dragged over or revert to default when file is dragged away
	/// </summary>
	/// <param name="dragEvent">Called drag event from text box</param>
	private static void ChangeCursorOnDrag(DragEventArgs dragEvent) {
		// Check if the data is a file
		dragEvent.Effects = dragEvent.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : // Show copy cursor
			DragDropEffects.None; // Revert to default
	}
	/// <summary>
	/// Grabs the SHA256 hash of a file and outputs it as a string
	/// </summary>
	/// <param name="filePath">The filepath of what file to grab the hash from</param>
	/// <returns>The SHA256 hash as a string</returns>
	private async Task<string> GrabHashFromFile(string filePath) {
		try {
			byte[] fileBytes = await File.ReadAllBytesAsync(filePath); // Read the file content
			byte[] fileHash = SHA256.HashData(fileBytes); // Hash the content
			string fileHashString = BitConverter.ToString(fileHash).Replace("-", ""); // Convert to string

			UpdateEmoji(); // Update emoji
			return fileHashString; // Return converted string
		} catch (Exception ex) {
			throw new Exception($"Could not grab hash from {filePath}", ex); // Throw ex for caller method to handle
		}
	}
	/// <summary>
	/// Grabs the SHA256 hash of a file and outputs it as a string
	/// </summary>
	/// <param name="dragEvent">Called drag event from text box</param>
	/// <returns>The SHA256 hash as a string</returns>
	private async Task<string> GrabHashFromFile(DragEventArgs dragEvent) {
		if (dragEvent.Data.GetDataPresent(DataFormats.FileDrop) && dragEvent.Data.GetData(DataFormats.FileDrop) is string[] paths) {
			// Allow only one file, and it must not be a directory
			if (paths.Length == 1 && File.Exists(paths[0])) {
				try {
					string filePath = paths[0]; // Single file selected, grab filepath
					byte[] fileBytes = await File.ReadAllBytesAsync(filePath); // Read the file content
					byte[] fileHash = SHA256.HashData(fileBytes); // Hash the content
					string fileHashString = BitConverter.ToString(fileHash).Replace("-", ""); // Convert to string

					UpdateEmoji(); // Update emoji
					return fileHashString; // Return converted string
				} catch (Exception ex) {
					throw new Exception($"Could not grab hash from {paths[0]}", ex); // Throw ex for caller method to handle
				}
			}
		}
		SystemSounds.Beep.Play(); // Invalid file, playing warning sound
		return string.Empty; // Return blank value
	}
	/// <summary>
	/// Used to highlight a text box control by adjusting the border brush and thickness
	/// </summary>
	/// <param name="textBoxControl">Text box control to highlight</param>
	/// <param name="highlightBrush">Solid color brush for border</param>
	/// <param name="thicknessUniformLength">Uniform length of border thickness</param>
	private static void HighlightTextBox(TextBox textBoxControl, SolidColorBrush highlightBrush, double thicknessUniformLength) {
		textBoxControl.BorderBrush = highlightBrush;
		textBoxControl.BorderThickness = new Thickness(thicknessUniformLength);
	}
	/// <summary>
	/// Resets the border brush and thickness of a TextBox
	/// </summary>
	/// <param name="textBox">The TextBox to change border parameters</param>
	private static void ResetTextBoxStyle(TextBox textBox) {
		textBox.BorderBrush = SystemColors.ControlDarkBrush;
		textBox.BorderThickness = new Thickness(1);
	}
	/// <summary>
	/// Updates the emoji label based on the comparison of the left and right text box contents
	/// Emoji Logic:
	/// - 😺 (Happy kitty): Shown when either text box is empty
	/// - 🙀 (Shocked kitty): Shown when both text boxes have different values
	/// - 😻 (Loving kitty): Shown when both text boxes have the same value
	/// </summary>
	private void UpdateEmoji() {
		string leftTextBoxContent = LeftTextBox.Text;
		string rightTextBoxContent = RightTextBox.Text;

		if (leftTextBoxContent == string.Empty || rightTextBoxContent == string.Empty) {
			// One or both text boxes are empty, show default happy kitty
			EmojiLabel.Content = "😺";
			return;
		}

		// Show shocked kitty if different, or loving kitty if same
		EmojiLabel.Content = leftTextBoxContent == rightTextBoxContent ? "😻" : "🙀";
	}
}