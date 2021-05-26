using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigTableScript : MonoBehaviour {

	/// <summary>
	/// I took the digits 1-9 and used random.org to generate a random 1000 digit number with them. 
	/// Then I took all the letters and shuffled them.
	/// I used random.org to decide which letters should get 39, and which 38.
	/// Then I took the 38 first occurences of the number 1, replaced them with the first letter.
	/// Then the 39 next occurences, replaced them with the second letter, etc. Until I run out of 1s, then move on to 2. Etc Etc.
	/// Afterwards I did some manual shuffling to make it so similar sounding/looking letters dont show up together as much. 
	/// The manual table I made manually, as well as populating GridSequence.cs, using a quick output to determine where the first and last of each letter occurence are.
	/// This was a late night project in my random unity calculations project. This code does not have good naming. However, it's probably a good starting point if one wants to add ruleseed to this.
	/// </summary>



	string numbers = "5833899887537611474228372523959426434112179954749171549653966856113853574164253884881517367813745887963824928113467236431457183654895336635463265549365654892784926244665861919357231228535144316182965743336685474689613114252593922329156749592763841872159412829113332537651784597943577795936319355275143725568465441253687633121242534545262251317424221765631823618391682938366856542934884496686532858384948358758763362787627258461721577735839892229288568329621269287654318382522988596444256864159715487615852313696373744591736744859533779518755812375438358285419695631778183141186198181484221586842985815959144332867991449971377836717186435888575966347268441238267711445639323476596883346765547435517791498827919498735841612722997176783734992528699223781867376859914894374415134921856564534746296643599274713295682143464788788769845848833428419282576253998235688275732473225592672448544792422361974871577813938278776689833799272356644142617795476418965475968632353945416187122277291286748913715975227883";

	int[] counts = { 38, 39, 38, 39, 39, 39, 39, 39, 38, 38, 39, 38, 38, 38, 39, 38, 38, 38, 38, 39, 38, 39, 39, 38, 39, 38, 109 };

	string letters = "QXVTEHCIJALRPDZGFSBNUWKMOY";

	string newNumbers = "RUHHUMMUUSRHSZQQJSJVVUHSVRVHMRMJVZJHJQQVQSMMRJSJMQSQRJMZRHMZZURZQQHURHRSJQZJVRHUUJUUQRQSHZSUQHSJRUUSMZHUVJMVUQQHJZSVHZJHQJRSQUHZRJUMRHHZZHRJZHVZRRJMHZRZPJUMVSUJMVZVJAGGPUGQMQMCPSVCQTTUPCPQAACQGQUTMGPSACCCGGUPASAGUMGQCQQATPTPMCMTTCTOQPGSAOPOTSGCUAQUSTQPOAQTUTOQQCCCTPCSGPQSUAPOSOACPSSSOPOCGCQOCPPTBPQACBTPPGUAGPAAQTPCGWBGCCQTQTATPCAPAPTGTTPXCXBATATTXBGPGCXWTCGXWCOXGWTOCWCGGWPGPATOCAWWAAOGGWGPCTWPWCWAOAWCPWBPWBGCCGTBWBGTBTPWAGXBTXPBBBCPWCOWOTTTOTWWDGWCTOGTXEGOEWBGDAIXWIWEDEEOWWDOGAAAEDGWFAXDOBXDAWBFXDWDEIXIFOFIBIBAADOXBIFBAAWDODIIBBODXWBDDWXEIBDLIWIDWEWDLXOFODFIXBBWXWIXLXXWFXOKXKXLKLEEXDKFKLEOKDKXDODOXLLIIEKFBOOXLLOOBXIBBKIFBXBXKFLIDKKKDBDOFFILBEFKLLXEIKEFBBXXLLDFIOIEILBFDYFKKIILFNFDDLNLIDDXNNYXLYKKENYXYLYKNIDKLVFVENEEYYNVNFNKINILYYEDEKFYYEEINKVKFNINFKDYYVLKYLINLLVDVILYEVKDFDFLZILNLFEYFFLIZYYENLNVIEYZFKEVLILFLNKKNKKNFYKLZKLKKIJREKRVYEKEZNFEZJYYMEJZFMMENZNJERNJEEZZYHFNHRRMZRRNYHRHHJFVYNRMNVZNNMVJYJMHNMNNSSMYMJJNYYHNHJZSSRRVRHSVNNYZRNSRVMYSZRUZYSMSJHJZJYRZRVSVMUVHHHUUHYVHMSURMYVJUVZYUZHHUMMJ";

	// Use this for initialization
	void Start() {
		int[] firsts = new int[26];
		int[] lasts = new int[26];

		string current = "";

		//string paste = "";
		int ugh = -1;
		foreach (char l in letters) {
			ugh++;
			int first = -1;
			int last = -1;
			for (int i = 0; i < 1000; i++) {
				if (newNumbers[i] == l) {
					if (first == -1) {
						first = i;
					}
					if (i - last > 200) {
						first = i;
						if (last != -1) {
							break;
						}
					}
					last = i;
				}
			}
			firsts[ugh] = first;
			lasts[ugh] = last;
			//paste = paste + l + " " + first + " " + last + "\n";
			if (last < first) {
				current += l;
			}
		}
		//Debug.Log(paste);


		string pasta = "";
		Debug.Log(current);
		pasta += 0 + " " + current + "\n";
		for (int i = 0; i < 1000; i++) {
			for (int j = 0; j < letters.Length; j++) {
				if (firsts[j] == i) {
					Debug.Log("add " + letters[j]);
					current += letters[j];
					pasta += i + " " + current + "\n";
				}
				if (lasts[j] == i) {
					Debug.Log(letters[j]);
					current = current.Replace(letters[j].ToString(), "");
					pasta += i + " " + current + "\n";
				}
			}
		}
		Debug.Log(pasta);

		return;
		string newNums = numbers;
		int letter = 0;
		int letterCount = 0;
		for (int number = 1; number <= 9; number++) {
			char numChar = number.ToString()[0];
			for (int i = 0; i < 1000; i++) {
				if (numbers[i] == numChar) {
					newNums = newNums.Remove(i, 1).Insert(i, letters[letter].ToString());
					letterCount++;
				}
				if (letterCount == counts[letter]) {
					letter++;
					letterCount = 0;
				}
			}
		}
		Debug.Log(newNums);
	}
}
