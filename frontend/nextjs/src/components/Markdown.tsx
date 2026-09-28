import { Fragment } from "react";

// Minimal, safe markdown renderer (headings, lists, bold, paragraphs) — no raw HTML injection.
export function Markdown({ content }: { content: string }) {
  const lines = content.replace(/\r\n/g, "\n").split("\n");
  const blocks: React.ReactNode[] = [];
  let list: string[] = [];

  const flushList = (key: string) => {
    if (list.length === 0) return;
    blocks.push(
      <ul key={key}>
        {list.map((item, i) => (
          <li key={i}>{renderInline(item)}</li>
        ))}
      </ul>
    );
    list = [];
  };

  lines.forEach((raw, index) => {
    const line = raw.trimEnd();
    if (line.startsWith("### ")) {
      flushList(`l-${index}`);
      blocks.push(<h3 key={index}>{renderInline(line.slice(4))}</h3>);
    } else if (line.startsWith("## ")) {
      flushList(`l-${index}`);
      blocks.push(<h2 key={index}>{renderInline(line.slice(3))}</h2>);
    } else if (line.startsWith("# ")) {
      flushList(`l-${index}`);
      blocks.push(<h1 key={index}>{renderInline(line.slice(2))}</h1>);
    } else if (line.startsWith("- ")) {
      list.push(line.slice(2));
    } else if (line.trim() === "") {
      flushList(`l-${index}`);
    } else {
      flushList(`l-${index}`);
      blocks.push(<p key={index}>{renderInline(line)}</p>);
    }
  });
  flushList("l-final");

  return <div className="prose-content">{blocks}</div>;
}

function renderInline(text: string): React.ReactNode {
  const parts = text.split(/(\*\*[^*]+\*\*)/g);
  return parts.map((part, i) => {
    if (part.startsWith("**") && part.endsWith("**")) {
      return <strong key={i}>{part.slice(2, -2)}</strong>;
    }
    return <Fragment key={i}>{part}</Fragment>;
  });
}
