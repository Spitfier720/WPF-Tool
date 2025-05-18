// Form2.cs
namespace Windows_Forms_Project
{
    public partial class Form2 : Form
    {
        private string Input1 => textBoxInput1.Text;
        private string Input2 => textBoxInput2.Text;
        private string Input3 => textBoxInput3.Text;

        public Form2()
        {
            InitializeComponent();
        }

        private void buttonSubmit_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public (string, string, string) GetInputs()
        {
            return (Input1, Input2, Input3);
        }
    }
}