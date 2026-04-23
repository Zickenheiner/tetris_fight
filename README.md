# Tetris Fight

Jeu Tetris multijoueur en temps réel, construit avec **C# / .NET 10** et **Avalonia UI 12**. Trois modes de jeu : solo, versus IA, et multijoueur en réseau local via TCP.

---

## Fonctionnalités

### Modes de jeu

| Mode                   | Description                                     |
| ---------------------- | ----------------------------------------------- |
| **Solo**               | Tetris classique contre la gravité              |
| **VS IA**              | Deux plateaux côte à côte, joueur contre une IA |
| **Multijoueur réseau** | Partie en réseau local (TCP, port 55001)        |

### Mécanique de jeu

- 7 types de tétrominos (I, O, T, S, Z, J, L) avec rotations dans les deux sens
- Grille 20 × 10 cases
- Accélération progressive de la gravité (500 ms → 200 ms minimum)
- Aperçu de la pièce fantôme (activable/désactivable)
- Hard drop (touche Espace)
- Système de score : 100 / 300 / 500 / 800 points selon le nombre de lignes effacées
- Animations : dissolution lors de l'effacement de lignes, remplissage rouge en game over
- Pause / reprise

### Système de sabotage

- Jauge 0–4 qui se remplit quand l'adversaire efface des lignes
- Jauge pleine → activation : l'adversaire est forcé de placer une pièce spécifique
- Synchronisé en réseau pour un jeu équitable

### Intelligence artificielle

- Évaluation heuristique : hauteur, trous, irrégularité, prédiction de lignes
- Test des 4 rotations × 14 colonnes par pièce
- 30 % de taux d'erreur pour un comportement humain
- Délai de réflexion de 800 ms
- Peut être mise en pause indépendamment

### Audio

- Musique de menu et musique de jeu en boucle
- Tempo dynamique synchronisé avec la vitesse du jeu (taux jusqu'à × 1,4)
- Audio natif macOS via AVAudioPlayer (silencieux sur les autres plateformes)

### Réseau

- Architecture hôte / client TCP sur port 55001
- Détection automatique de l'IP locale
- Synchronisation de la graine RNG pour des parties déterministes
- Mesure de latence (ping RTT toutes les 2 secondes)
- Gestion de la déconnexion

---

## Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- macOS, Windows 10+ ou Linux (fonctionnalités audio uniquement sur macOS)

---

## Installation et lancement

```bash
# Cloner le dépôt
git clone <url-du-repo>
cd tetris_fight

# Restaurer les dépendances
dotnet restore

# Lancer en mode développement
dotnet run

# Compiler en mode Release
dotnet build -c Release
```

---

## Commandes de jeu

| Touche    | Action               |
| --------- | -------------------- |
| `←` / `→` | Déplacer la pièce    |
| `↓`       | Descente accélérée   |
| `↑`       | Rotation horaire     |
| `Z`       | Rotation antihoraire |
| `Espace`  | Hard drop            |
| `ESC`     | Pause / reprise      |

---

## Architecture

Le projet suit une **Clean Architecture organisée par feature** (vertical slices).

```
tetris_fight/
├── Features/
│   ├── Board/          # Plateau, pièces, logique de jeu
│   │   ├── Domain/         # Entités : Tetromino, BoardState, GameInput
│   │   ├── Application/    # Services : BoardService, InputQueueService
│   │   ├── Infrastructure/ # GameLoopService, AiPlayerService
│   │   └── Presentation/   # ViewModels + vues Avalonia
│   ├── Audio/          # Gestion de la musique
│   ├── Menu/           # Navigation et menu principal
│   ├── Network/        # Multijoueur TCP
│   └── Shared/         # Abstractions partagées (RelayCommand, etc.)
├── App.axaml(.cs)      # Bootstrap Avalonia
├── MainWindow.axaml    # Fenêtre principale et navigation
└── Program.cs          # Point d'entrée
```

**Règles d'architecture :**

- Les dépendances vont uniquement vers l'intérieur : `Presentation → Application → Domain`
- Aucune logique de jeu dans les code-behinds, tout passe par les ViewModels
- Pas de référence directe entre features — communication via `Shared/` ou événements de domaine

---

## Stack technique

| Composant              | Version                  |
| ---------------------- | ------------------------ |
| .NET                   | 10.0                     |
| Avalonia UI            | 12.0.1                   |
| Avalonia.Themes.Fluent | 12.0.1                   |
| Avalonia.Fonts.Inter   | 12.0.1                   |
| Pattern UI             | MVVM + bindings compilés |

---

## Contribuer

1. Créer une branche depuis `dev` : `git checkout -b feat/ma-fonctionnalite`
2. Implémenter en respectant l'architecture Clean Architecture par feature
3. S'assurer que `dotnet build` passe sans erreur ni warning
4. Ouvrir une Pull Request vers `dev`
