import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { HubConnectionBuilder } from '@microsoft/signalr';

export default function PollResults() {
    const { code } = useParams();
    const [results, setResults] = useState<any>(null);
    const [liveQuestion, setLiveQuestion] = useState('');

    useEffect(() => {
        // Initial data hydration fetch
        fetch(`https://localhost:7168/api/polls/${code}/results`)
            .then(res => res.json())
            .then(data => setResults(data));

        // Connect to WebSocket via SignalR
        const connection = new HubConnectionBuilder()
            .withUrl('https://localhost:7168/hubs/poll')
            .withAutomaticReconnect()
            .build();

        connection.start()
            .then(() => {
                // 🟢 Only invoke if the connection successfully opened
                if (connection.state === "Connected") {
                    connection.invoke('JoinPollRoom', code)
                        .catch(err => console.error("Error joining room:", err));
                }
            })
            .catch(err => console.error('Connection structural crash: ', err));

        connection.on('BroadcastUpdate', (updatedSummary: any) => {
            setResults(updatedSummary);
        });

        return () => {
            // 🟢 Safely leave only if we are actively connected
            if (connection.state === "Connected") {
                connection.invoke('LeavePollRoom', code).catch(e => { });
            }
            connection.off('BroadcastUpdate');
            connection.stop();
        };
    }, [code]);

    const handleForceClose = async () => {
        await fetch(`https://localhost:7168/api/polls/${code}/close`, { method: 'POST' });
    };

    const handleSendQuestion = async () => {
        if (!liveQuestion.trim()) return;
        await fetch(`https://localhost:7168/api/polls/${code}/questions`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ content: liveQuestion })
        });
        setLiveQuestion('');
    };

    if (!results) return <div className="text-center p-8 text-slate-400">Loading live socket data stream...</div>;

    const totalVotes = results.distribution.reduce((acc: number, item: any) => acc + item.count, 0);

    return (
        <div className="space-y-6 max-w-xl mx-auto">
            <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm space-y-4">
                <div className="flex justify-between items-start">
                    <h2 className="text-xl font-bold text-slate-800">{results.question}</h2>
                    {results.isClosed ? (
                        <span className="bg-red-100 text-red-800 text-xs px-2.5 py-1 rounded-full font-bold">Concluded</span>
                    ) : (
                        <span className="bg-emerald-100 text-emerald-800 text-xs px-2.5 py-1 rounded-full font-bold animate-pulse">● Live Updates</span>
                    )}
                </div>

                {/* Dynamic bar charts with smooth animated inline CSS widths */}
                {results.type !== 3 && (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '12px', margin: '16px 0' }}>
                        {results.distribution.map((item: any, i: number) => {
                            const pct = totalVotes > 0 ? Math.round((item.count / totalVotes) * 100) : 0;
                            return (
                                <div key={i} style={{ display: 'flex', flexDirection: 'column', gap: '4px' }}>

                                    {/* 🟢 FIXED: Native CSS Flexbox splitting the elements to opposite sides */}
                                    <div style={{
                                        display: 'flex',
                                        justifyContent: 'space-between',
                                        alignItems: 'center',
                                        fontSize: '14px',
                                        fontWeight: '600',
                                        color: '#334155'
                                    }}>
                                        <span>{item.optionText || item.option}</span>
                                        <span style={{ color: '#6366f1' }}>{item.count} votes ({pct}%)</span>
                                    </div>

                                    {/* Progress Bar background track */}
                                    <div style={{ width: '100%', backgroundColor: '#f1f5f9', height: '24px', borderRadius: '6px', overflow: 'hidden', position: 'relative' }}>
                                        {/* Dynamic filled area */}
                                        <div style={{ backgroundColor: '#6366f1', height: '100%', width: `${pct}%`, transition: 'width 0.5s ease-out' }} />
                                    </div>

                                </div>
                            );
                        })}
                    </div>
                )}

                {results.type === 3 && (
                    <div className="space-y-2">
                        <h3 className="text-sm font-bold text-slate-700">Open-Ended Responses ({results.openTextResponses.length})</h3>
                        <div className="max-h-60 overflow-y-auto space-y-1.5 pr-1">
                            {results.openTextResponses.map((txt: string, i: number) => (
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

            {/* Anonymous Interactive Stream Component (Distinction Layer) */}
            <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm space-y-4">
                <h3 className="text-sm font-bold text-slate-800">Audience Live Q&A Stream</h3>
                <div className="flex gap-2">
                    <input className="flex-1 px-3 py-1.5 text-xs border rounded-lg focus:outline-indigo-500" value={liveQuestion} onChange={e => setLiveQuestion(e.target.value)} placeholder="Submit an anonymous question to the screen..." />
                    <button className="bg-slate-900 text-white text-xs px-4 py-1.5 rounded-lg font-medium hover:bg-slate-800" onClick={handleSendQuestion}>Submit</button>
                </div>

                <div className="space-y-2">
                    {results.questions.map((q: any) => (
                        <div key={q.id} className={`p-3 rounded-lg border flex justify-between items-center text-xs ${q.isPinned ? 'bg-amber-50 border-amber-300' : 'bg-slate-50'}`}>
                            <span className="font-medium text-slate-700">{q.content}</span>
                            <div className="flex items-center gap-2">
                                {q.isPinned && <span className="text-amber-600 font-bold text-[10px] uppercase tracking-wider">Pinned</span>}
                                <span className="bg-white border px-2 py-0.5 rounded text-slate-500">▲ {q.upvotes}</span>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
}