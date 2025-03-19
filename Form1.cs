using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace OrderCombiner {
    public partial class Form1 : Form {
        string outputFile = "";
        string firstFileContent = "";
        string secondFileContent = "";
        int count = 0;
        int[] size = { 18, 36, 54, 72, 90, 108, 126, 144, 162, 180, 198, 216, 234, 396, 504, 612 };
        List<string> fileContents;
        public Form1() {
            InitializeComponent();
        }
        string EditCount(string toBeEdited) {
            int editAfter = toBeEdited.IndexOf("<quantity>") + 10;
            int editBefore = toBeEdited.IndexOf("</quantity>");
            StringBuilder editedText = new StringBuilder(toBeEdited);
            editedText.Remove(editAfter, (editBefore - editAfter));
            editedText.Insert(editAfter, count);
            return editedText.ToString();
        }
        string EditBracket(string toBeEdited) {
            int editAfter = toBeEdited.IndexOf("<bracket>") + 9;
            int editBefore = toBeEdited.IndexOf("</bracket>");
            StringBuilder editedText = new StringBuilder(toBeEdited);
            editedText.Remove(editAfter, (editBefore - editAfter));
            int setBracket = 0;
            for (int i = 0; i < size.Length; i++) {
                if (count > size[i]) {
                    setBracket = size[i + 1];
                }
            }
            editedText.Insert(editAfter, setBracket);
            return editedText.ToString();
        }
        string ChopAfter(string toBeChopped) {
            string choppedText = "";
            int chopAfter = toBeChopped.LastIndexOf("</fronts>");
            if (chopAfter > 0) {
                choppedText = toBeChopped.Substring(0, chopAfter);
            }
            return choppedText;
        }
        string ChopBefore(string toBeChopped) {
            string choppedText = "";
            int chopBefore = toBeChopped.IndexOf("<fronts>");
            if (chopBefore > 0) {
                choppedText = toBeChopped.Substring(toBeChopped.IndexOf("<fronts>") + 8);
            }
            return choppedText;
        }
        char getFirstDigit(int num) {
            char[] numHolder = num.ToString().ToCharArray();
            return numHolder[0];
        }
        char getSecondDigit(int num) {
            char[] numHolder = num.ToString().ToCharArray();
            return numHolder[1];
        }
        char getThirdDigit(int num) {
            char[] numHolder = num.ToString().ToCharArray();
            return numHolder[2];
        }
        string ParseString(string inputString) {
            StringBuilder buildingString = new StringBuilder(inputString);
            List<int> deletions = new List<int>();
            for (int i = 1; i < buildingString.Length - 2; i++) {
                //checks to see if each character is in a card id list
                if (Char.IsDigit(buildingString[i]) && (buildingString[i - 1] == '[' || buildingString[i - 1] == ',')) {
                    //checks to see how long the number is and changes it into the 'count' number
                    buildingString[i] = getFirstDigit(count);
                    if (Char.IsDigit(buildingString[i + 1]) && count > 9) {
                        buildingString[i + 1] = getSecondDigit(count);
                        if (Char.IsDigit(buildingString[i + 2])) {
                            buildingString[i + 2] = getThirdDigit(count);
                            i++;
                        }
                        else if (Char.IsDigit(buildingString[i + 2]) && count <= 99) {
                            deletions.Add(i + 2);
                        }
                        else if (!Char.IsDigit(buildingString[i + 2]) && count > 99) {
                            buildingString.Insert(i + 2, count.ToString()[2]);
                            i++;
                        }
                        i++;
                    }
                    else if (Char.IsDigit(buildingString[i + 1]) && count <= 9) {
                        //saves indexs of the strings for numbers to delete once modification is complete
                        deletions.Add(i + 1);
                    }
                    else if (!Char.IsDigit(buildingString[i + 1]) && count > 9) {
                        buildingString.Insert(i + 1, count.ToString()[1]);
                        i++;
                    }
                    Console.WriteLine("cycle: " + i + "count: " + count);
                    count++;
                }

            }
            for (int i = deletions.Count - 1; i >= 0; i--) {
                buildingString.Remove(deletions[i], 1);
            }
            return buildingString.ToString();
        }
        void SelectMultipleFiles() {
            var numberOfFiles = 6;
            for (int i = 0; i < numberOfFiles; i++) {
                fileContents.Add(openFile());
            }

            SelectMultipleFilesInternal(fileContents);
        }
        void SelectFiles() {
            firstFileContent = openFile();
            secondFileContent = openFile();

            SelectFilesInternal(firstFileContent, secondFileContent);
        }
        string openFile() {
            string returnString = "";
            using (OpenFileDialog openFileDialog = new OpenFileDialog()) {
                openFileDialog.Filter = "xml files (*.xml)|*.xml";

                if (openFileDialog.ShowDialog() == DialogResult.OK) {
                    var fileStream = openFileDialog.OpenFile();
                    using (StreamReader reader = new StreamReader(fileStream)) {

                        returnString = reader.ReadToEnd();
                    }
                }
            }
            return returnString;
        }
        void ResetVariables() {
            outputFile = "";
            count = 0;
            firstFileContent = "";
            secondFileContent = "";
            fileContents = new List<string>();
        }
        private void button1_Click(object sender, EventArgs e) {
            if (firstFileContent.Length > 10 && secondFileContent.Length > 10 || fileContents.Count > 1) {
                XDocument doc = XDocument.Parse(outputFile);
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "XML-File | *.xml";
                if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                    doc.Save(saveFileDialog.FileName);
                    ResetVariables();
                }
            }
            else {
                MessageBox.Show("You do not have two files selected, please select two valid XML files");
            }
        }
        private void button2_Click(object sender, EventArgs e) {
            ResetVariables();
            SelectFiles();
        }
        private void button3_Click(object sender, EventArgs e) {
            ResetVariables();
            SelectMultipleFiles();
        }

        private void Form1_Load(object sender, EventArgs e) {

        }
        int GetQuantity(List<XElement> xmlDetails) {
            return Int32.Parse(xmlDetails.Descendants().First(x => x.Name == "quantity").Value);
        }
        void SelectFilesInternal(string firstFileContent, string secondFileContent) {
            var firstXml = XDocument.Parse(firstFileContent);

            var firstXmlCardbacks = firstXml.Descendants().Where(x => x.Name == "cardback").ToList();

            var firstXmlDetails = firstXml.Descendants().Where(x => x.Name == "details").ToList();
            var firstXmlFrontCards = firstXml.Descendants().Where(x => x.Name == "fronts")
                .Descendants().Where(x => x.Name == "card").ToList();
            var firstXmlBackCards = firstXml.Descendants().Where(x => x.Name == "backs")
                .Descendants().Where(x => x.Name == "card").ToList();

            var firstXmlCount = GetQuantity(firstXmlDetails);

            var secondXml = XDocument.Parse(secondFileContent);
            var secondXmlDetails = secondXml.Descendants().Where(x => x.Name == "details").ToList();
            var secondXmlFrontCards = secondXml.Descendants().Where(x => x.Name == "fronts")
                .Descendants().Where(x => x.Name == "card").ToList();
            var secondXmlBackCards = secondXml.Descendants().Where(x => x.Name == "backs")
                .Descendants().Where(x => x.Name == "card").ToList();

            var secondXmlCount = GetQuantity(secondXmlDetails);

            // edit xml elements

            // edit quantity with total from first and second xmls, and edit bracket to hold new quantity
            var editedXmlDetails = firstXmlDetails;
            int totalCount = firstXmlCount + secondXmlCount;
            int setBracket = 0;
            for (int i = 0; i < size.Length; i++) {
                if (totalCount > size[i]) {
                    if (i == size.Length - 1) {
                        MessageBox.Show("Total cards cannot exceed 612, you have " + totalCount + " cards between your two files.");
                        return;
                    }
                    setBracket = size[i + 1];
                }
            }
            editedXmlDetails.Descendants().Where(x => x.Name == "quantity").First().SetValue(totalCount.ToString());
            editedXmlDetails.Descendants().Where(x => x.Name == "bracket").First().SetValue(setBracket.ToString());

            // update second front card slot values based on first xml count
            var secondXmlFrontCardsCount = secondXmlFrontCards.Count;
            for ( int i = 0; i < secondXmlFrontCardsCount; i++ ) {
                var slotElement = secondXmlFrontCards[i].Descendants().First(x => x.Name == "slots");
                var slotValue = slotElement.Value;
                var slotValues = slotValue.Split(',')
                    .Select(Int32.Parse)
                    .Select(x => x + firstXmlCount);
                var editedSlotValue = string.Join(",", slotValues);
                slotElement.SetValue(editedSlotValue);
            }

            // append first fronts with second fronts
            var editedXmlFrontCards = firstXmlFrontCards.Concat(secondXmlFrontCards);

            // update second back card slot values based on first xml count
            var secondXmlBackCardsCount = secondXmlBackCards.Count;
            for (int i = 0; i < secondXmlBackCardsCount; i++) {
                var slotElement = secondXmlBackCards[i].Descendants().First(x => x.Name == "slots");
                var slotValue = slotElement.Value;
                var slotValues = slotValue.Split(',')
                    .Select(Int32.Parse)
                    .Select(x => x + firstXmlCount);
                var editedSlotValue = string.Join(",", slotValues);
                slotElement.SetValue(editedSlotValue);
            }

            // append first backs with second backs
            var editedXmlBackCards = firstXmlBackCards.Concat(secondXmlBackCards);

            // just use first xml cardback?
            var editedXmlCardbacks = firstXmlCardbacks;

            StringBuilder sb = new StringBuilder();
            XmlWriterSettings xws = new XmlWriterSettings();
            xws.OmitXmlDeclaration = true;
            xws.Indent = true;

            using (XmlWriter xw = XmlWriter.Create(sb, xws)) {
                XDocument doc;
                if (editedXmlBackCards.Any()) {
                    doc = new XDocument(
                        new XElement("order",
                            editedXmlDetails,
                            new XElement("fronts",
                                editedXmlFrontCards
                            ),
                            new XElement("backs",
                                editedXmlBackCards
                            ),
                            editedXmlCardbacks
                        )
                    );
                }
                else {
                    doc = new XDocument(
                        new XElement("order",
                            editedXmlDetails,
                            new XElement("fronts",
                                editedXmlFrontCards
                            ),
                            editedXmlCardbacks
                        )
                    );
                }

                doc.WriteTo(xw);
            }

            if (totalCount > 612) {
                MessageBox.Show("Total cards cannot exceed 612, you have " + totalCount + " cards between your two files.");
                ResetVariables();
                return;
            }

            outputFile = sb.ToString();

            return;
        }
        void SelectMultipleFilesInternal(List<string> fileContents) {
            int i = 0;

            // find first non-empty file
            while (i < fileContents.Count - 1 && fileContents[i].Length == 0) {
                i++;
            }

            // combine current file with next until finished
            outputFile = fileContents[i];
            while (i < fileContents.Count - 1) {
                var next = fileContents[i + 1];

                if (next.Any()) {
                    SelectFilesInternal(outputFile, next);
                }

                i++;
            }
        }
    }
}
