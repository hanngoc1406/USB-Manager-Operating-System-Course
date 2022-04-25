
using System.Management;
using System.Runtime.InteropServices;

namespace UsbManager_Test
{
    public partial class Form1 : Form
    {
        public static class USBManager
        {

            const int OPEN_EXISTING = 3;
            const uint GENERIC_READ = 0x80000000;
            const uint GENERIC_WRITE = 0x40000000;
            // Default value to Ejects media from a SCSI device
            const uint IOCTL_STORAGE_EJECT_MEDIA = 0x2D4808;

            [DllImport("kernel32")]
            // function creates or opens objects and returns a handle that can be used to access the object
            // Ham tao hoac mo mot doi tuong va return 1 handle co the dung de truy cap doi tuong
            // Doi tuong duoc dung o day la 
            private static extern IntPtr CreateFile(
                string filename,
                uint desiredAccess,
                uint shareMode,
                IntPtr securityAttributes,
                int creationDisposition,
                int flagsAndAttributes,
                IntPtr templateFile);

            [DllImport("kernel32")]
            // Closes an open object handle.
            // Dong mot doi tuong handle dang mo
            private static extern int CloseHandle(IntPtr handle);

            [DllImport("kernel32")]

            //
            private static extern int DeviceIoControl(
                IntPtr deviceHandle, 
                uint ioControlCode, 
                IntPtr inBuffer, 
                int inBufferSize, 
                IntPtr outBuffer, 
                int outBufferSize, 
                ref int bytesReturned, 
                IntPtr overlapped);

            // Eject USB
            public static string EjectUSB(char driveLetterCharacter)
            {
                // DOS device paths: \\.\driveLetterCharacter:
                string pathfordrive = "\\\\.\\" + driveLetterCharacter + ":";

                // Create handle to use device
                IntPtr handle = CreateFile(pathfordrive, GENERIC_READ | GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                

                // if CreateFile()'s return value < 0 that mean the function was unable to create the specified file.
                if ((long)handle == -1)
                {
                    int error = Marshal.GetLastWin32Error();
                    var result = string.Format("Không thể ngắt kết nối USB({0}) - Error code {1}", driveLetterCharacter, error);
                    return result;
                }

                // Just make some variable for require ref keyword
                int dummy = 0;

                // Just function to send control code
                DeviceIoControl(handle, IOCTL_STORAGE_EJECT_MEDIA, IntPtr.Zero, 0, IntPtr.Zero, 0, ref dummy, IntPtr.Zero);

                // Just close handle
                CloseHandle(handle);
                return "Đã ngắt kết nốt usb thành công";
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady & d.DriveType == DriveType.Removable);

            if (drives.FirstOrDefault() != null)
            {
                double totalSize = (drives.FirstOrDefault().TotalSize) / (1024.0 * 1024.0 * 1024.0);
                double freeSpace = (drives.FirstOrDefault().AvailableFreeSpace) / (1024.0 * 1024.0 * 1024.0);
                listBox1.Items.Add(string.Format("{0}{1} - {2} free of {3}", drives.FirstOrDefault().Name, drives.FirstOrDefault().VolumeLabel, freeSpace.ToString("N1"), totalSize.ToString("N1")));
            }
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady & d.DriveType == DriveType.Removable);

            if (drives.FirstOrDefault() != null)
            {
                string status = USBManager.EjectUSB(Convert.ToChar(drives.FirstOrDefault().Name.Replace(":\\", "")));
                DialogResult check = MessageBox.Show(status);

                if (DialogResult.OK == check && status == "Đã tháo usb thành công")
                {
                    listBox1.Items.Clear();
                }
            }
        }
    }
}