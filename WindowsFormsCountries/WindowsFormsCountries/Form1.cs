using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WindowsFormsCountries
{
    public partial class Form1 : Form
    {
        public List<Country> lCountries; //prazna javna lista
        public Form1()
        {
            InitializeComponent();
            //Debug.WriteLine(sUrl); --->provjerava vrijednost varijabli
            //Trace.WriteLine(sUrl); --->provjerava vrijednost varijabli

            //DATA GRID
            lCountries = GetCountries();
            dataGridViewCountries.DataSource = lCountries;

            //COMBO BOX FILTRIRANJE
            List<String> lRegions = lCountries.Where(o => o.sRegion != "").Select(o => o.sRegion).Distinct().ToList(); //linq za dohvaćanje jedinstvenih vrijednosti iz liste
            lRegions.Insert(0, "Svi kontinenti");
            comboBoxRegion.DataSource = lRegions;

            //COMBO BOX SORTIRANJE
            List<string> lSortCriterias = new List<string>()
                {
                    "‐",
                    "Glavni grad",
                    "Naziv",
                    "Broj stanovnika",
                    "Povrsina"
                };
            comboBoxSort.DataSource = lSortCriterias;
        }
        public List<Country> GetCountries()
        {
            List<Country> lRESTCountries = new List<Country>();
            string sUrl = System.Configuration.ConfigurationManager.AppSettings["RestApiUrl"];
            string sJson = CallRestMethod(sUrl);

            JArray json = JArray.Parse(sJson);
            foreach (JObject item in json)
            {
                //čitanje vrijednosti iz jsona
                string code = (string)item.GetValue("alpha2Code");
                string name = (string)item.GetValue("name");
                string capital = (string)item.GetValue("capital");
                int population = (int)item.GetValue("population");
                //float area=(float)item.GetValue("area");
                float area = -1;
                if (item.GetValue("area").Type == JTokenType.Null)
                {
                    area = 0;
                }
                else
                {
                    area = (float)item.GetValue("area");
                }
                string region = (string)item.GetValue("region");

                //dodavanje objekata u listu
                lRESTCountries.Add(new Country
                {
                    sCode = code,
                    sName = name,
                    sCapital = capital,
                    nPopulation = population,
                    fArea = area,
                    sRegion = region,
                });
            }
            return lRESTCountries;
        }
            
        public static string CallRestMethod(string url)
        {
            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
            webrequest.Method = "GET";
            webrequest.ContentType = "application/x-www-form-urlencoded";
            //webrequest.Headers.Add("Username", "xyz");
            //webrequest.Headers.Add("Password", "abc");
            HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
            Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader responseStream = new StreamReader(webresponse.GetResponseStream(),enc);
            string result = string.Empty;
            result = responseStream.ReadToEnd();
            webresponse.Close();
            return result;
        }

        private void comboBoxRegion_SelectedIndexChanged(object sender, EventArgs e)
        {
            //čitanje odabrane vrijednosti
            string sRegion = (string)comboBoxRegion.SelectedItem;

            lCountries = GetCountries();
            if(sRegion!="Svi kontinenti")
            {
                lCountries = lCountries.Where(o => o.sRegion == sRegion).ToList();
                dataGridViewCountries.DataSource = lCountries;
            }
            else
            {
                dataGridViewCountries.DataSource = lCountries;
            }
        }

        private void comboBoxSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            string sCriteria = (string)comboBoxSort.SelectedItem;

            switch(sCriteria)
            {
                case "-":
                    break;
                case "Glavni grad":
                    dataGridViewCountries.DataSource = lCountries.OrderBy(o => o.sCapital).ToList();
                    break;
                case "Naziv":
                    dataGridViewCountries.DataSource = lCountries.OrderBy(o => o.sName).ToList();
                    break;
                case "Broj stanovnika":
                    dataGridViewCountries.DataSource = lCountries.OrderByDescending(o => o.nPopulation).ToList();
                    break;
                case "Povrsina":
                    dataGridViewCountries.DataSource = lCountries.OrderByDescending(o => o.fArea).ToList();
                    break;

            }
        }

        private void btnPretrazi_Click(object sender, EventArgs e)
        {
            string sPretraga = inptPretrazi.Text;
            var vPretraga = from c in lCountries where c.sName.Contains(sPretraga) select c;
            List<Country> lPretrazeneDrzave = vPretraga.ToList();
            dataGridViewCountries.DataSource = lPretrazeneDrzave.OrderBy(o => o.sName).ToList();
        }

        private void btnSpremi_Click(object sender, EventArgs e)
        {
            string sKod = inptKod.Text;
            string sNaziv = inptNaziv.Text;
            string sGlGrad = inptGlavniGrad.Text;
            string nBrStanovnika = inptBrojStanovnika.Text;
            string fPovrsina = inptPovrsina.Text;
        }
    }
}
