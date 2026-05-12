import { type LeaderboardEntry } from "../types/participant";
import "./Leaderboard.css";

type LeaderboardProps = {
    entries: LeaderboardEntry[];
    currentParticipantName?: string;
};

export default function Leaderboard({ entries, currentParticipantName }: LeaderboardProps) {
    if (!entries || entries.length === 0) {
        return (
            <div className="leaderboard-empty">
                <p>A ranglista még üres.</p>
            </div>
        );
    }

    return (
        <div className="leaderboard-container">
            <h2 className="leaderboard-title">Emberi Ranglista</h2>
            <ul className="leaderboard-list">
                {entries.map((entry, idx) => {
                    const isMe = entry.participantName === currentParticipantName;
                    let medal = "";
                    let rowClass = "leaderboard-row";

                    // Top 3 positions logic
                    if (entry.position === 1) { 
                        medal = "🥇"; 
                        rowClass += " gold"; 
                    } else if (entry.position === 2) { 
                        medal = "🥈"; 
                        rowClass += " silver"; 
                    } else if (entry.position === 3) { 
                        medal = "🥉"; 
                        rowClass += " bronze"; 
                    }

                    // Highlight the current user
                    if (isMe) {
                        rowClass += " is-me";
                    }

                    return (
                        <li key={idx} className={rowClass}>
                            <span className="lb-pos">
                                {entry.position}. {medal}
                            </span>
                            <span className="lb-name">
                                {entry.participantName} {isMe ? "(Te)" : ""}
                            </span>
                            <span className="lb-score">
                                {entry.correctAnswersCount} pont
                            </span>
                        </li>
                    );
                })}
            </ul>
        </div>
    );
}

