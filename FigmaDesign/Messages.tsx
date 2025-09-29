import { useState, useEffect, useRef } from "react";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Badge } from "./ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "./ui/tabs";
import { ScrollArea } from "./ui/scroll-area";
import { Avatar, AvatarFallback, AvatarImage } from "./ui/avatar";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import {
  Search,
  Users,
  MessageSquare,
  Plus,
  Settings,
  Phone,
  Video,
  MoreHorizontal,
  Pin,
  Bell,
  BellOff,
  UserPlus,
  Edit,
  Trash,
  Send,
  Smile,
  Paperclip,
  ThumbsUp,
  Eye,
  EyeOff
} from "lucide-react";

// Types
interface User {
  id: string;
  name: string;
  avatar?: string;
  status: "online" | "offline" | "away" | "busy";
  lastSeen?: string;
  role?: string;
  department?: string;
}

interface Message {
  id: string;
  senderId: string;
  content: string;
  timestamp: string;
  edited?: boolean;
  reactions?: { emoji: string; users: string[] }[];
  seen: boolean;
  mentions?: string[];
}

interface ChatThread {
  id: string;
  participants: string[];
  name?: string;
  isGroup: boolean;
  lastMessage?: Message;
  unreadCount: number;
  pinned: boolean;
  muted: boolean;
  avatar?: string;
}

interface Group {
  id: string;
  name: string;
  description?: string;
  members: string[];
  avatar?: string;
  isPublic: boolean;
  adminIds: string[];
  createdAt: string;
}

interface MessagesProps {
  onChatOpen: (threadId: string) => void;
}

// Mock data
const mockUsers: User[] = [
  { id: "1", name: "Sarah Wilson", status: "online", role: "Production Manager", department: "Operations" },
  { id: "2", name: "Mike Rodriguez", status: "online", role: "Machine Operator", department: "Production" },
  { id: "3", name: "Jennifer Chen", status: "away", role: "Quality Supervisor", department: "QA" },
  { id: "4", name: "David Thompson", status: "offline", lastSeen: "2 hours ago", role: "Logistics Coordinator", department: "Shipping" },
  { id: "5", name: "Lisa Park", status: "busy", role: "Inventory Manager", department: "Warehouse" },
  { id: "6", name: "James Miller", status: "online", role: "Safety Officer", department: "Safety" },
  { id: "7", name: "Emily Davis", status: "offline", lastSeen: "1 day ago", role: "Maintenance Tech", department: "Maintenance" },
  { id: "8", name: "Robert Johnson", status: "online", role: "Shift Supervisor", department: "Operations" }
];

const mockGroups: Group[] = [
  {
    id: "g1",
    name: "Production Team",
    description: "Daily production coordination and updates",
    members: ["1", "2", "3", "8"],
    isPublic: false,
    adminIds: ["1", "8"],
    createdAt: "2024-01-15T08:00:00Z"
  },
  {
    id: "g2",
    name: "Safety Committee",
    description: "Safety incidents, training, and compliance discussions",
    members: ["1", "6", "7", "8"],
    isPublic: true,
    adminIds: ["6"],
    createdAt: "2024-01-10T10:00:00Z"
  },
  {
    id: "g3",
    name: "Shift Handoff",
    description: "Communication between shifts",
    members: ["1", "2", "4", "5", "8"],
    isPublic: false,
    adminIds: ["1"],
    createdAt: "2024-01-20T06:00:00Z"
  }
];

const mockThreads: ChatThread[] = [
  {
    id: "t1",
    participants: ["1", "2"],
    isGroup: false,
    lastMessage: {
      id: "m1",
      senderId: "2",
      content: "The slitter line is ready for the next job. Quality check passed.",
      timestamp: "2024-01-31T14:30:00Z",
      seen: false,
      reactions: [{ emoji: "👍", users: ["1"] }]
    },
    unreadCount: 1,
    pinned: false,
    muted: false
  },
  {
    id: "t2",
    participants: ["1", "3", "6"],
    name: "Safety Review",
    isGroup: true,
    lastMessage: {
      id: "m2",
      senderId: "6",
      content: "Please review the new safety protocols before tomorrow's meeting.",
      timestamp: "2024-01-31T13:45:00Z",
      seen: true
    },
    unreadCount: 0,
    pinned: true,
    muted: false
  },
  {
    id: "t3",
    participants: ["1", "4"],
    isGroup: false,
    lastMessage: {
      id: "m3",
      senderId: "4",
      content: "Load T-401 is ready for dispatch. Customer confirmed delivery window.",
      timestamp: "2024-01-31T12:15:00Z",
      seen: true
    },
    unreadCount: 0,
    pinned: false,
    muted: false
  }
];

export function Messages({ onChatOpen }: MessagesProps) {
  const [activeTab, setActiveTab] = useState("threads");
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedThread, setSelectedThread] = useState<string | null>(null);
  const [selectedThreadData, setSelectedThreadData] = useState<{
    id: string;
    name: string;
    isGroup: boolean;
    participants: User[];
  } | null>(null);

  const filteredThreads = mockThreads.filter(thread => {
    if (!searchQuery) return true;
    
    const participantNames = thread.participants
      .map(id => mockUsers.find(u => u.id === id)?.name || "")
      .join(" ");
    
    return participantNames.toLowerCase().includes(searchQuery.toLowerCase()) ||
           thread.name?.toLowerCase().includes(searchQuery.toLowerCase());
  });

  const filteredContacts = mockUsers.filter(user =>
    user.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    user.role?.toLowerCase().includes(searchQuery.toLowerCase()) ||
    user.department?.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const filteredGroups = mockGroups.filter(group =>
    group.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    group.description?.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const getStatusColor = (status: string) => {
    switch (status) {
      case "online": return "bg-green-500";
      case "away": return "bg-yellow-500";
      case "busy": return "bg-red-500";
      default: return "bg-gray-400";
    }
  };

  const formatTime = (timestamp: string) => {
    const date = new Date(timestamp);
    const now = new Date();
    const diffHours = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60));
    
    if (diffHours < 1) return "Just now";
    if (diffHours < 24) return `${diffHours}h ago`;
    if (diffHours < 168) return `${Math.floor(diffHours / 24)}d ago`;
    return date.toLocaleDateString();
  };

  const handleThreadSelect = (threadId: string, threadName: string, isGroup: boolean, participants: User[]) => {
    setSelectedThread(threadId);
    setSelectedThreadData({
      id: threadId,
      name: threadName,
      isGroup,
      participants
    });
    onChatOpen(threadId);
  };

  return (
    <div className="h-full flex bg-white">
      {/* Left Sidebar - Chat List */}
      <div className="w-80 border-r flex flex-col bg-gray-50">
        <div className="p-4 border-b bg-white">
          <div className="flex items-center justify-between mb-3">
            <h2 className="font-semibold">Messages</h2>
            <Button size="sm">
              <Plus className="h-4 w-4 mr-2" />
              New Chat
            </Button>
          </div>
          
          <div className="relative">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Search messages..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-9"
            />
          </div>
        </div>

        <Tabs value={activeTab} onValueChange={setActiveTab} className="flex-1 flex flex-col">
          <div className="px-4 pt-2">
            <TabsList className="grid w-full grid-cols-3">
              <TabsTrigger value="threads" className="text-xs relative">
                Chats
                {mockThreads.reduce((sum, t) => sum + t.unreadCount, 0) > 0 && (
                  <Badge className="absolute -top-1 -right-1 h-4 w-4 rounded-full p-0 text-xs bg-red-500 text-white">
                    {mockThreads.reduce((sum, t) => sum + t.unreadCount, 0)}
                  </Badge>
                )}
              </TabsTrigger>
              <TabsTrigger value="contacts" className="text-xs">
                Contacts
              </TabsTrigger>
              <TabsTrigger value="groups" className="text-xs">Groups</TabsTrigger>
            </TabsList>
          </div>

          <div className="flex-1 overflow-hidden">
            <ScrollArea className="h-full">
              <TabsContent value="threads" className="mt-0 p-2">
                {filteredThreads.length === 0 ? (
                  <div className="text-center py-12 text-muted-foreground">
                    <MessageSquare className="h-12 w-12 mx-auto mb-4 opacity-50" />
                    <p>No chat threads found</p>
                    <p className="text-sm">Start a new conversation with a colleague</p>
                  </div>
                ) : (
                  <div className="space-y-1">
                    {filteredThreads.map((thread) => {
                      const otherParticipant = thread.isGroup 
                        ? null 
                        : mockUsers.find(u => u.id === thread.participants.find(id => id !== "1"));
                      
                      return (
                        <div
                          key={thread.id}
                          className={`p-3 cursor-pointer transition-all hover:bg-white rounded-lg ${
                            selectedThread === thread.id ? 'bg-white border-r-2 border-teal-500' : ''
                          }`}
                          onClick={() => {
                            const threadName = thread.isGroup 
                              ? thread.name || "Group Chat"
                              : otherParticipant?.name || "Unknown";
                            const participants = thread.isGroup 
                              ? thread.participants.map(id => mockUsers.find(u => u.id === id)).filter(Boolean) as User[]
                              : [mockUsers[0], otherParticipant].filter(Boolean) as User[];
                            
                            handleThreadSelect(thread.id, threadName, thread.isGroup, participants);
                          }}
                        >
                          <div className="flex items-start gap-3">
                            <div className="relative">
                              <Avatar className="h-10 w-10">
                                <AvatarFallback>
                                  {thread.isGroup 
                                    ? thread.name?.slice(0, 2).toUpperCase()
                                    : otherParticipant?.name.slice(0, 2).toUpperCase()
                                  }
                                </AvatarFallback>
                              </Avatar>
                              {!thread.isGroup && otherParticipant && (
                                <div className={`absolute -bottom-1 -right-1 h-3 w-3 rounded-full border-2 border-white ${getStatusColor(otherParticipant.status)}`} />
                              )}
                            </div>
                            
                            <div className="flex-1 min-w-0">
                              <div className="flex items-center justify-between mb-1">
                                <h3 className="font-medium text-sm truncate">
                                  {thread.isGroup ? thread.name : otherParticipant?.name}
                                </h3>
                                <div className="flex items-center gap-1">
                                  {thread.pinned && <Pin className="h-3 w-3 text-muted-foreground" />}
                                  {thread.muted && <BellOff className="h-3 w-3 text-muted-foreground" />}
                                  {thread.unreadCount > 0 && (
                                    <Badge className="h-4 w-4 p-0 text-xs bg-red-500 text-white">
                                      {thread.unreadCount}
                                    </Badge>
                                  )}
                                </div>
                              </div>
                              
                              {thread.lastMessage && (
                                <div className="flex items-center justify-between">
                                  <p className="text-xs text-muted-foreground truncate flex-1">
                                    {thread.lastMessage.senderId === "1" ? "You: " : ""}
                                    {thread.lastMessage.content}
                                  </p>
                                  <span className="text-xs text-muted-foreground ml-2">
                                    {formatTime(thread.lastMessage.timestamp)}
                                  </span>
                                </div>
                              )}
                            </div>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                )}
              </TabsContent>

              <TabsContent value="contacts" className="mt-0 p-2">
                {filteredContacts.length === 0 ? (
                  <div className="text-center py-12 text-muted-foreground">
                    <Users className="h-12 w-12 mx-auto mb-4 opacity-50" />
                    <p>No contacts found</p>
                  </div>
                ) : (
                  <div className="space-y-1">
                    {filteredContacts.map((user) => (
                      <div
                        key={user.id}
                        className={`p-3 cursor-pointer transition-all hover:bg-white rounded-lg ${
                          selectedThread === `t_${user.id}` ? 'bg-white border-r-2 border-teal-500' : ''
                        }`}
                        onClick={() => {
                          const existingThread = mockThreads.find(t => 
                            !t.isGroup && t.participants.includes(user.id)
                          );
                          const threadId = existingThread?.id || `t_${user.id}_${Date.now()}`;
                          const participants = [mockUsers[0], user];
                          
                          handleThreadSelect(threadId, user.name, false, participants);
                        }}
                      >
                        <div className="flex items-center gap-3">
                          <div className="relative">
                            <Avatar className="h-10 w-10">
                              <AvatarFallback>{user.name.slice(0, 2).toUpperCase()}</AvatarFallback>
                            </Avatar>
                            <div className={`absolute -bottom-1 -right-1 h-3 w-3 rounded-full border-2 border-white ${getStatusColor(user.status)}`} />
                          </div>
                          
                          <div className="flex-1 min-w-0">
                            <p className="font-medium text-sm">{user.name}</p>
                            <p className="text-xs text-muted-foreground">
                              {user.role} • {user.department}
                            </p>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </TabsContent>

              <TabsContent value="groups" className="mt-0 p-2">
                {filteredGroups.length === 0 ? (
                  <div className="text-center py-12 text-muted-foreground">
                    <Users className="h-12 w-12 mx-auto mb-4 opacity-50" />
                    <p>No groups found</p>
                    <Button className="mt-4" size="sm">
                      <Plus className="h-4 w-4 mr-2" />
                      Create Group
                    </Button>
                  </div>
                ) : (
                  <div className="space-y-1">
                    {filteredGroups.map((group) => (
                      <div
                        key={group.id}
                        className={`p-3 cursor-pointer transition-all hover:bg-white rounded-lg ${
                          selectedThread === `tg_${group.id}` ? 'bg-white border-r-2 border-teal-500' : ''
                        }`}
                        onClick={() => {
                          const existingThread = mockThreads.find(t => 
                            t.isGroup && t.name === group.name
                          );
                          const threadId = existingThread?.id || `tg_${group.id}_${Date.now()}`;
                          const participants = group.members.map(id => mockUsers.find(u => u.id === id)).filter(Boolean) as User[];
                          
                          handleThreadSelect(threadId, group.name, true, participants);
                        }}
                      >
                        <div className="flex items-start gap-3">
                          <Avatar className="h-10 w-10">
                            <AvatarFallback>
                              <Users className="h-5 w-5" />
                            </AvatarFallback>
                          </Avatar>
                          
                          <div className="flex-1 min-w-0">
                            <div className="flex items-center justify-between mb-1">
                              <h3 className="font-medium text-sm">{group.name}</h3>
                              {group.isPublic && (
                                <Badge variant="outline" className="text-xs">Public</Badge>
                              )}
                            </div>
                            
                            <p className="text-xs text-muted-foreground mb-1">
                              {group.description}
                            </p>
                            
                            <p className="text-xs text-muted-foreground">
                              {group.members.length} members
                            </p>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </TabsContent>
            </ScrollArea>
          </div>
        </Tabs>
      </div>

      {/* Right Panel - Chat Interface */}
      <div className="flex-1 flex flex-col">
        {selectedThreadData ? (
          <MessagingInterface 
            threadId={selectedThreadData.id}
            threadName={selectedThreadData.name}
            isGroup={selectedThreadData.isGroup}
            participants={selectedThreadData.participants}
          />
        ) : (
          <div className="flex-1 flex items-center justify-center bg-gray-50">
            <div className="text-center">
              <MessageSquare className="h-16 w-16 mx-auto mb-4 text-gray-400" />
              <h3 className="font-medium text-gray-900 mb-2">Welcome to Messages</h3>
              <p className="text-gray-500 max-w-sm">
                Select a conversation from the sidebar to start messaging with your team members.
              </p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

// Chat Interface Component
interface MessagingInterfaceProps {
  threadId: string;
  threadName: string;
  isGroup: boolean;
  participants: User[];
}

function MessagingInterface({ threadId, threadName, isGroup, participants }: MessagingInterfaceProps) {
  const [message, setMessage] = useState("");
  const [messages, setMessages] = useState<Message[]>([]);
  const [isTyping, setIsTyping] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  // Mock messages for demo
  useEffect(() => {
    const mockMessages: { [key: string]: Message[] } = {
      "t1": [
        {
          id: "m1",
          senderId: "2",
          content: "Hey, how's the production schedule looking for today?",
          timestamp: "2024-01-31T09:00:00Z",
          seen: true
        },
        {
          id: "m2",
          senderId: "1",
          content: "Looking good! We're on track to meet the quotas. The slitter line should be ready by 2 PM.",
          timestamp: "2024-01-31T09:05:00Z",
          seen: true
        },
        {
          id: "m3",
          senderId: "2",
          content: "Perfect! I'll prep the quality check station. @Sarah any special requirements for the BC Steel order?",
          timestamp: "2024-01-31T09:10:00Z",
          mentions: ["1"],
          seen: true
        },
        {
          id: "m4",
          senderId: "1",
          content: "Yes, they want extra tight tolerances on the width. Make sure the slitter guides are calibrated to +/- 0.5mm",
          timestamp: "2024-01-31T09:15:00Z",
          seen: true,
          reactions: [{ emoji: "👍", users: ["2"] }]
        }
      ],
      "t2": [
        {
          id: "m5",
          senderId: "6",
          content: "Team, we need to review the updated safety protocols before tomorrow's meeting.",
          timestamp: "2024-01-31T13:00:00Z",
          seen: true
        },
        {
          id: "m6",
          senderId: "3",
          content: "I've reviewed the lockout/tagout procedures. Looking good overall.",
          timestamp: "2024-01-31T13:15:00Z",
          seen: true
        }
      ]
    };

    setMessages(mockMessages[threadId] || []);
  }, [threadId]);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const sendMessage = () => {
    if (!message.trim()) return;

    const newMessage: Message = {
      id: `m_${Date.now()}`,
      senderId: "1",
      content: message,
      timestamp: new Date().toISOString(),
      seen: false
    };

    setMessages(prev => [...prev, newMessage]);
    setMessage("");
    
    setTimeout(() => {
      setMessages(prev => prev.map(m => 
        m.id === newMessage.id ? { ...m, seen: true } : m
      ));
    }, 2000);
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      sendMessage();
    }
  };

  const formatTime = (timestamp: string) => {
    const date = new Date(timestamp);
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  };

  const getUserById = (id: string) => participants.find(p => p.id === id);
  const otherParticipant = !isGroup && participants.find(p => p.id !== "1");

  return (
    <div className="flex flex-col h-full">
      {/* Chat Header */}
      <div className="flex items-center justify-between p-4 border-b bg-white">
        <div className="flex items-center gap-3">
          <Avatar className="h-10 w-10">
            <AvatarFallback>
              {isGroup 
                ? threadName.slice(0, 2).toUpperCase()
                : otherParticipant?.name.slice(0, 2).toUpperCase()
              }
            </AvatarFallback>
          </Avatar>
          
          <div>
            <h3 className="font-medium">{threadName}</h3>
            <p className="text-sm text-muted-foreground">
              {isGroup 
                ? `${participants.length} members`
                : otherParticipant?.status === "online" ? "Online" : "Last seen recently"
              }
            </p>
          </div>
        </div>
        
        <div className="flex items-center gap-2">
          <Button variant="ghost" size="sm">
            <Phone className="h-4 w-4" />
          </Button>
          <Button variant="ghost" size="sm">
            <Video className="h-4 w-4" />
          </Button>
          <Button variant="ghost" size="sm">
            <MoreHorizontal className="h-4 w-4" />
          </Button>
        </div>
      </div>

      {/* Messages Area */}
      <ScrollArea className="flex-1 p-4">
        <div className="space-y-4">
          {messages.map((msg, index) => {
            const sender = getUserById(msg.senderId);
            const isCurrentUser = msg.senderId === "1";
            const showSender = isGroup && !isCurrentUser && (index === 0 || messages[index - 1].senderId !== msg.senderId);
            
            return (
              <div key={msg.id} className={`flex gap-3 ${isCurrentUser ? 'justify-end' : 'justify-start'}`}>
                {!isCurrentUser && (
                  <Avatar className="h-8 w-8 mt-1">
                    <AvatarFallback className="text-xs">
                      {sender?.name.slice(0, 2).toUpperCase()}
                    </AvatarFallback>
                  </Avatar>
                )}
                
                <div className={`max-w-[70%] ${isCurrentUser ? 'items-end' : 'items-start'} flex flex-col`}>
                  {showSender && (
                    <span className="text-xs text-muted-foreground mb-1">
                      {sender?.name}
                    </span>
                  )}
                  
                  <div
                    className={`px-4 py-2 rounded-2xl text-sm ${
                      isCurrentUser
                        ? 'bg-teal-600 text-white rounded-br-md'
                        : 'bg-gray-100 text-gray-900 rounded-bl-md'
                    }`}
                  >
                    {msg.content}
                  </div>
                  
                  {msg.reactions && msg.reactions.length > 0 && (
                    <div className="flex gap-1 mt-1">
                      {msg.reactions.map((reaction, idx) => (
                        <div
                          key={idx}
                          className="text-xs px-2 py-1 rounded-full bg-gray-100 border"
                        >
                          {reaction.emoji} {reaction.users.length}
                        </div>
                      ))}
                    </div>
                  )}
                  
                  <span className="text-xs text-muted-foreground mt-1">
                    {formatTime(msg.timestamp)}
                  </span>
                </div>
              </div>
            );
          })}
          
          {isTyping && (
            <div className="flex gap-3">
              <Avatar className="h-8 w-8">
                <AvatarFallback>...</AvatarFallback>
              </Avatar>
              <div className="bg-gray-100 px-4 py-2 rounded-2xl rounded-bl-md">
                <div className="flex gap-1">
                  <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce" />
                  <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: '0.1s' }} />
                  <div className="w-2 h-2 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: '0.2s' }} />
                </div>
              </div>
            </div>
          )}
          
          <div ref={messagesEndRef} />
        </div>
      </ScrollArea>

      {/* Message Input */}
      <div className="p-4 border-t bg-white">
        <div className="flex items-end gap-3">
          <Button variant="ghost" size="sm">
            <Paperclip className="h-4 w-4" />
          </Button>
          
          <div className="flex-1">
            <Input
              value={message}
              onChange={(e) => setMessage(e.target.value)}
              onKeyPress={handleKeyPress}
              placeholder={`Message ${isGroup ? threadName : otherParticipant?.name}...`}
              className="rounded-full border-gray-300 focus:border-teal-500 focus:ring-teal-500"
            />
          </div>
          
          <Button variant="ghost" size="sm">
            <Smile className="h-4 w-4" />
          </Button>
          
          <Button 
            onClick={sendMessage}
            disabled={!message.trim()}
            size="sm"
            className="bg-teal-600 hover:bg-teal-700 rounded-full"
          >
            <Send className="h-4 w-4" />
          </Button>
        </div>
      </div>
    </div>
  );
}