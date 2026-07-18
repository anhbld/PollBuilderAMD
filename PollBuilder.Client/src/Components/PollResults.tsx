import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { HubConnectionBuilder } from '@microsoft/signalr';

export default function PollResults() {
    const { code } = useParams();
    const [results, setResults] = useState<any>(null);
    const [liveQuestion, setLiveQuestion] = useState('');

    // 🟢 Uses the VITE_API_URL set in your docker-compose.yml
    const apiBase = "http://localhost:5000";

    useEffect(() => {
        // 🟢 Replaced ngrok URL with dynamic apiBase
        fetch(`${apiBase}/api/polls/${code}/results`)
            .then(res => res.json())
            .then(data => setResults(data));

        // 🟢 Simplified Hub connection; standard negotiation works best in local/Docker networks
        const connection = new HubConnectionBuilder()
            .withUrl(`${apiBase}/hubs/poll`)
            .withAutomaticReconnect()
            .build();

        connection.start()
            .then(() => {
                if (connection.state === "Connected") {
                    connection.invoke('JoinPollRoom', code)
                        .catch((err: any) => console.error("Error joining room:", err));
                }
            })
            .catch((err: any) => console.error('Connection failed: ', err));

        connection.on('BroadcastUpdate', (updatedSummary: any) => {
            setResults(updatedSummary);
        });

        return () => {
            if (connection.state === "Connected") {
                connection.invoke('LeavePollRoom', code).catch(() => { });
            }
            connection.off('BroadcastUpdate');
            connection.stop();
        };
    }, [code, apiBase]);

    const handleForceClose = async () => {
        await fetch(`${apiBase}/api/polls/${code}/close`, { method: 'POST' });
    };

    const handleSendQuestion = async () => {
        if (!liveQuestion.trim()) return;
        await fetch(`${apiBase}/api/polls/${code}/questions`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ content: liveQuestion })
        });
        setLiveQuestion('');
    };

    if (!results) return <div className="text-center p-8 text-slate-400">Loading live socket data stream...</div>;

    // 🟢 Defensive Coding: Fallback to empty array if properties are missing
    const distribution = results.distribution || [];
    const questions = results.questions || [];
    const openTextResponses = results.openTextResponses || [];

    const totalVotes = distribution.reduce((acc: number, item: any) => acc + (item.count || 0), 0);

    return (
        <div className="space-y-6 max-w-xl mx-auto text-slate-800">
            <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm space-y-4">
                <div className="flex justify-between items-start">
                    <h2 className="text-xl font-bold text-slate-800">{results.question}</h2>
                    {results.isClosed ? (
                        <span className="bg-red-100 text-red-800 text-xs px-2.5 py-1 rounded-full font-bold">Concluded</span>
                    ) : (
                        <span className="bg-emerald-100 text-emerald-800 text-xs px-2.5 py-1 rounded-full font-bold animate-pulse">● Live Updates</span>
                    )}
                </div>

                {results.type !== 3 && (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '12px', margin: '16px 0' }}>
                        {distribution.map((item: any, i: number) => {
                            const count = item.count || 0;
                            const pct = totalVotes > 0 ? Math.round((count / totalVotes) * 100) : 0;
                            return (
                                <div key={i} style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>
                                    <div style={{
                                        display: 'flex',
                                        justifyContent: 'space-between',
                                        alignItems: 'center',
                                        fontSize: '14px',
                                        fontWeight: '600',
                                        color: '#334155'
                                    }}>
                                        <span>{item.optionText || item.option || `Option ${i + 1}`}</span>
                                        <span style={{ color: '#6366f1' }}>{count} votes ({pct}%)</span>
                                    </div>
                                    <div style={{ width: '100%', backgroundColor: '#f1f5f9', height: '24px', borderRadius: '6px', overflow: 'hidden', position: 'relative' }}>
                                        <div style={{ backgroundColor: '#6366f1', height: '100%', width: `${pct}%`, transition: 'width 0.5s ease-out' }} />
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                )}

                {results.type === 3 && (
                    <div className="space-y-2">
                        <h3 className="text-sm font-bold text-slate-700">Open-Ended Responses ({openTextResponses.length})</h3>
                        <div className="max-h-60 overflow-y-auto space-y-1.5 pr-1">
                            {openTextResponses.map((txt: string, i: number) => (
                                <div key={i} className="bg-slate-50 border p-2.5 text-xs rounded-lg text-slate-700 font-mono">{txt}</div>
                            ))}
                        </div>
                    </div>
                )}

                {!results.isClosed && (
                    <button className="w-full border border-red-200 text-red-600 hover:bg-red-50 text-xs py-1.5 rounded-lg font-medium transition" onClick={handleForceClose}>
                        Close Voting Window Now
                    </button>
                )}
            </div>

            <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm space-y-4">
                <h3 className="text-sm font-bold text-slate-800">Audience Live Q&A Stream</h3>
                <div className="flex gap-2">
                    <input className="flex-1 px-3 py-1.5 text-xs border rounded-lg focus:outline-indigo-500 bg-white text-slate-900" value={liveQuestion} onChange={e => setLiveQuestion(e.target.value)} placeholder="Submit an anonymous question to the screen..." />
                    <button className="bg-slate-900 text-white text-xs px-4 py-1.5 rounded-lg font-medium hover:bg-slate-800" onClick={handleSendQuestion}>Submit</button>
                </div>

                <div className="space-y-2">
                    {questions.map((q: any, i: number) => (
                        <div key={q.id || q.Id || i} className={`p-3 rounded-lg border flex justify-between items-center text-xs ${q.isPinned || q.IsPinned ? 'bg-amber-50 border-amber-300' : 'bg-slate-50'}`}>
                            <span className="font-medium text-slate-700">{q.content || q.Content}</span>
                            <div className="flex items-center gap-2">
                                {(q.isPinned || q.IsPinned) && <span className="text-amber-600 font-bold text-[10px] uppercase tracking-wider">Pinned</span>}
                                <span className="bg-white border px-2 py-0.5 rounded text-slate-500">▲ {q.upvotes ?? q.Upvotes ?? 0}</span>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
}