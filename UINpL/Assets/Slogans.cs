using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slogans : MonoBehaviour {

	[SerializeField] [TextArea] string _textUninteracted;
	[SerializeField] [TextArea] string _textContinued;
	[SerializeField] [TextArea] string _textSolved;
	[SerializeField] [TextArea] string _textStrike;

	[SerializeField] string[] _cakes = {
		"Angel food cake",
		"Apple cake",
		"Babka",
		"Battenburg cake",
		"Baumkuchen",
		"Birthday cake",
		"Black Forest cake",
		"Buccellato",
		"Bundt cake",
		"Butter cake",
		"Butterfly cake",
		"Carrot cake",
		"Cheesecake",
		"Chocolate cake",
		"Christmas cake",
		"Chiffon cake",
		"Croquembouche",
		"Cupcake",
		"Date and walnut loaf",
		"Devil's food cake",
		"Eccles cake",
		"Fairy cake",
		"Fifteens",
		"Fruit cake",
		"Sponge cake",
		"Génoise Cake",
		"Gingerbread",
		"Gob",
		"Gooey butter cake",
		"Honey cake",
		"Hot milk cake",
		"Hummingbird cake",
		"Ice cream cake",
		"Jaffa Cakes",
		"Suncake",
		"Mooncake",
		"Orehnjac(a",
		"Pancake",
		"Panettone",
		"Petit fours",
		"Pineapple Upside Down Cake",
		"Pound cake",
		"Queen Elizabeth cake",
		"Red bean cake",
		"Red velvet cake",
		"Rhubarb cake",
		"Sachertorte",
		"St. Honore Cake",
		"Simnel cake",
		"Spice cake",
		"German chocolate cake",
		"Stack cake",
		"Leavened cake, Hefekuchen",
		"Tarte Tatin",
		"Teacake",
		"Tres leches cake",
		"Vanilla slice",
		"Vanilla Crazy Cake",
		"Victoria Sponge",
		"Wedding cake",
	};

	TextMesh _textMesh;

	void Start() {
		_textMesh = GetComponent<TextMesh>();
	}

	public void SetStage(int stage) {
		switch (stage) {
			case 0:
				_textMesh.text = _textUninteracted;
				break;
			case 1:
				_textMesh.text = _textContinued;
				break;
			case 2:
				_textMesh.text = _textStrike;
				break;
			default:
				string[] cakesList = _cakes.Shuffle();
				_textMesh.text = string.Format(_textSolved, cakesList[0], cakesList[1], cakesList[2], cakesList[3], cakesList[4]);
				break;
		}
	}


}


