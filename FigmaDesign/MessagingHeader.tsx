import { useState } from "react";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { MessageCircle } from "lucide-react";
import { MessageDropdown } from "./MessagingSystem";

interface MessagingHeaderProps {
  onChatOpen: (threadId: string, threadName?: string, isGroup?: boolean, participants?: any[]) => void;
  onFullPageOpen: () => void;
}

export function MessagingHeader({ onChatOpen, onFullPageOpen }: MessagingHeaderProps) {
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);

  return (
    <div className="relative">
      <Button 
        variant="ghost" 
        size="sm" 
        className="relative"
        onClick={() => setIsDropdownOpen(!isDropdownOpen)}
      >
        <MessageCircle className="h-4 w-4" />
        <Badge className="absolute -top-1 -right-1 h-5 w-5 rounded-full p-0 text-xs bg-teal-600 text-white">
          2
        </Badge>
      </Button>
      
      <MessageDropdown
        isOpen={isDropdownOpen}
        onClose={() => setIsDropdownOpen(false)}
        onChatOpen={(threadId, threadName, isGroup, participants) => {
          onChatOpen(threadId, threadName, isGroup, participants);
          setIsDropdownOpen(false);
        }}
        onFullPageOpen={() => {
          setIsDropdownOpen(false);
          onFullPageOpen();
        }}
      />
    </div>
  );
}