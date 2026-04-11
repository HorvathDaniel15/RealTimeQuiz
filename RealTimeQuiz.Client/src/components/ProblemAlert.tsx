type Props = {
    message: string | null;
};

export default function ProblemAlert({ message }: Props) {
    if (!message) return null;

    return <p className="problem-alert">{message}</p>;
}
