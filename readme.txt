Cours :	Intelligence artificielle
Projet: IA pour Othello en utilisant l'algorithme min-max
Auteurs : Ombang Ndo Charles et Lovis Thomas
----------------------------------------------------------------------------------------------------
Implémentation de l'algorithme alpha-beta en suivant la structure donnée dans le cours d'IA
Niveau de profondeur par défaut est 5.

Fonction d'évaluation (inspiré de https://pastebin.com/XsbX4zKN):
	6 critères :
		1. grille de score constante, calcule du score des deux joueurs et on soustraie notre score à celui du score ennemi
		2. nombre total de disques sur le plateau de jeu
		3. nombre de disques en frontières, un disque en frontière est un disque adjacent à une case vide dans au moins une direction
		4. la mobilité indirecte du joueur, c'est-à-dire le nombre de coups possibles à jouer dans l'état actuel
		5. le nombre de coins pris
		6. le nombre de pions adjacents à un coin (en général mauvais coup)
		
	Pour les points 2, 3 et 4, un ratio est fait par rapport à l'ennemi. La valeur est négative si "la valeur" de l'ennemi est meilleur.