namespace Windows_Forms_Project
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void buttonOpenForm2_Click(object sender, EventArgs e)
        {
            using (Form2 form2 = new Form2())
            {
                if (form2.ShowDialog() == DialogResult.OK)
                {
                    var (userInput1, userInput2, userInput3) = form2.GetInputs();
                    labelDisplayInput.Text = $"Input 1: {userInput1}\nInput 2: {userInput2}\nInput 3: {userInput3}";
                }
            }
        }
    }
}
