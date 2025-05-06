<?php
require 'db.php';

if (basename($_SERVER['PHP_SELF']) === 'get_results.php') {
    $stmt = $pdo->prepare("
        SELECT 
            game_id,
            players.id AS player_id,
            players.username, 
            games.kronus,
            games.lyrion,
            games.mystara,
            games.eclipsia,
            games.fiora
        FROM games
        JOIN players ON games.player_id = players.id
        WHERE players.status = 'submitted'
    ");
    $stmt->execute();
    $players = $stmt->fetchAll(PDO::FETCH_ASSOC);

    $scores = array_fill_keys(array_column($players, 'player_id'), 0);

    for ($i = 0; $i < count($players); $i++) {
        for ($j = $i + 1; $j < count($players); $j++) {
            $playerA = $players[$i];
            $playerB = $players[$j];

            $planets = ['kronus', 'lyrion', 'mystara', 'eclipsia', 'fiora'];
            $winsA = 0;
            $winsB = 0;

            foreach ($planets as $planet) {
                $a = (int)$playerA[$planet];
                $b = (int)$playerB[$planet];
                if ($a > $b) $winsA++;
                elseif ($b > $a) $winsB++;
            }

            if ($winsA > $winsB) {
                $scores[$playerA['player_id']] += 2;
            } elseif ($winsB > $winsA) {
                $scores[$playerB['player_id']] += 2;
            } else {
                $scores[$playerA['player_id']] += 1;
                $scores[$playerB['player_id']] += 1;
            }
        }
    }

    foreach ($scores as $playerId => $score) {
        $update = $pdo->prepare("UPDATE games SET score = :score WHERE player_id = :player_id");
        $update->execute([
            ':score' => $score,
            ':player_id' => $playerId
        ]);
    }

    $stmt = $pdo->prepare("
        SELECT 
            players.id AS player_id,
            players.username, 
            games.score,
            games.kronus,
            games.lyrion,
            games.mystara,
            games.eclipsia,
            games.fiora
        FROM games
        JOIN players ON games.player_id = players.id
        WHERE players.status = 'submitted'
        ORDER BY games.score DESC
    ");
    $stmt->execute();
    $results = $stmt->fetchAll(PDO::FETCH_ASSOC);
    
    echo json_encode(["results" => $results]);
}
?>
