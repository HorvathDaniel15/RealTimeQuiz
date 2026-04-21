import "./matrix-loader.css";

type Props = {
    lines: string[];
    isSuccess?: boolean;
};

export default function MatrixLoader({ lines, isSuccess = false }: Props) {
    return (
        <div className="matrix-terminal" role="status" aria-live="polite">
            <div className="matrix-terminal-header">
                <span className="matrix-dot matrix-dot-red" />
                <span className="matrix-dot matrix-dot-yellow" />
                <span className="matrix-dot matrix-dot-green" />
            </div>
            <div className="matrix-terminal-body">
                {lines.map((line, index) => (
                    <div
                        key={`${line}-${index}`}
                        className={`matrix-terminal-line ${
                            isSuccess && index === lines.length - 1 ? "matrix-terminal-line-success" : ""
                        }`}
                    >
                        {line}
                        {index === lines.length - 1 && <span className="matrix-cursor">_</span>}
                    </div>
                ))}
            </div>
        </div>
    );
}

