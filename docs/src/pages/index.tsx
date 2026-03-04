import React from 'react';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Layout from '@theme/Layout';
import CodeBlock from '@theme/CodeBlock';

function HeroSection() {
  return (
    <header className="hero--turbo">
      <div className="hero__inner">
        <div className="hero__eyebrow">
          <span className="hero__eyebrow-dot" />
          Open Source .NET Library
        </div>
        <h1>
          Ship SPA-Like Apps with{' '}
          <span className="hero__gradient-text">Server-Rendered HTML</span>
        </h1>
        <p>
          Bring Hotwire Turbo and Stimulus to ASP.NET Core. Partial page updates,
          real-time streams over SignalR, and Razor Pages you already know &mdash;
          no JavaScript framework required.
        </p>
        <div className="hero__buttons">
          <Link className="hero__button--primary" to="/docs/getting-started">
            Get Started &rarr;
          </Link>
          <Link
            className="hero__button--outline"
            to="https://github.com/tombatron/Tombatron.Turbo">
            GitHub
          </Link>
        </div>
        <div className="hero__terminal">
          <div className="hero__terminal-header">
            <div className="hero__terminal-dots">
              <span /><span /><span />
            </div>
            <span className="hero__terminal-title">Program.cs</span>
          </div>
          <div className="hero__terminal-body">
            <code>
              <span className="hl-comment">{'// Three lines. That\'s it.'}</span>{'\n'}
              {'builder.Services.'}<span className="hl-method">AddTurbo</span>{'();'}{'\n'}
              {'app.'}<span className="hl-method">UseTurbo</span>{'();'}{'\n'}
              {'app.'}<span className="hl-method">MapTurboHub</span>{'();'}
            </code>
          </div>
        </div>
        <div className="hero__badges">
          <a href="https://www.nuget.org/packages/Tombatron.Turbo/">
            <img
              alt="NuGet"
              src="https://img.shields.io/nuget/v/Tombatron.Turbo.svg?style=flat-square&color=10B981"
            />
          </a>
          <a href="https://www.npmjs.com/package/@tombatron/turbo-signalr">
            <img
              alt="npm"
              src="https://img.shields.io/npm/v/@tombatron/turbo-signalr.svg?style=flat-square&color=06B6D4"
            />
          </a>
          <a href="https://github.com/tombatron/Tombatron.Turbo/actions/workflows/build.yml">
            <img
              alt="Build"
              src="https://img.shields.io/github/actions/workflow/status/tombatron/Tombatron.Turbo/build.yml?style=flat-square"
            />
          </a>
        </div>
      </div>
    </header>
  );
}

function PitchBar() {
  const stack = [
    'ASP.NET Core',
    'Razor Pages',
    'SignalR',
    'Tag Helpers',
    'Dependency Injection',
    'Source Generators',
  ];

  return (
    <section className="pitch-bar">
      <p className="pitch-bar__text">
        Already know Razor Pages? You're 90% there.
      </p>
      <p className="pitch-bar__sub">
        Tombatron.Turbo isn't a new framework to learn. It's a thin layer that
        makes your existing server-rendered pages feel like a SPA.
      </p>
      <div className="pitch-bar__stack">
        {stack.map((item, i) => (
          <span key={i} className="pitch-bar__stack-item">{item}</span>
        ))}
      </div>
    </section>
  );
}

const features = [
  {
    title: 'Turbo Frames',
    description:
      'Wrap any section in a <turbo-frame> and only that region updates on navigation. SPA-like speed with zero client-side routing.',
  },
  {
    title: 'Real-Time Streams',
    description:
      'Push live DOM updates over SignalR. Append, prepend, replace, or remove elements from any connected client with a single method call.',
  },
  {
    title: 'Source-Generated Partials',
    description:
      'The bundled source generator creates strongly-typed partial references at compile time. Typos become build errors, not runtime 500s.',
  },
  {
    title: 'Three-Line Setup',
    description:
      'AddTurbo(), UseTurbo(), MapTurboHub() — Turbo.js, the SignalR bridge, and signed stream tokens are wired up automatically.',
  },
  {
    title: 'Razor All the Way Down',
    description:
      'Tag helpers, partials, view models, DI — everything you already use. No component model, no virtual DOM, no new abstractions to learn.',
  },
  {
    title: 'Stimulus Controllers',
    description:
      'Convention-based controller discovery via import maps. Drop a JS file in wwwroot/controllers/ and it auto-registers, with hot reload in dev.',
  },
];

function FeaturesSection() {
  return (
    <section className="features">
      <h2>Why Tombatron.Turbo?</h2>
      <p className="features__subtitle">
        Everything you need to build rich, interactive web applications without
        leaving the server-rendered world.
      </p>
      <div className="features__grid">
        {features.map((feature, idx) => (
          <div key={idx} className="feature-card">
            <div className="feature-card__number">
              {String(idx + 1).padStart(2, '0')}
            </div>
            <h3>{feature.title}</h3>
            <p>{feature.description}</p>
          </div>
        ))}
      </div>
    </section>
  );
}

function CodeShowcase() {
  return (
    <section className="code-showcase">
      <div className="container">
        <h2>See It in Action</h2>
        <p className="code-showcase__subtitle">
          From setup to real-time broadcasts in under a minute.
        </p>
        <div className="code-showcase__grid">
          <div className="code-showcase__item">
            <div className="code-showcase__label">
              <span className="code-showcase__step">01</span>
              <h3>Register Services</h3>
            </div>
            <CodeBlock language="csharp">
{`// Program.cs
builder.Services.AddTurbo();
builder.Services.AddRazorPages();

app.UseTurbo();
app.MapTurboHub();
app.MapRazorPages();`}
            </CodeBlock>
          </div>
          <div className="code-showcase__item">
            <div className="code-showcase__label">
              <span className="code-showcase__step">02</span>
              <h3>Add a Turbo Frame</h3>
            </div>
            <CodeBlock language="html">
{`<turbo-frame id="cart" src="/cart"
             loading="lazy">
  <p>Loading cart...</p>
</turbo-frame>`}
            </CodeBlock>
          </div>
          <div className="code-showcase__item">
            <div className="code-showcase__label">
              <span className="code-showcase__step">03</span>
              <h3>Broadcast in Real-Time</h3>
            </div>
            <CodeBlock language="csharp">
{`await _turbo.Stream("orders", async b =>
{
    await b.AppendAsync(
        "list",
        Partials.OrderRow,
        newOrder);
});`}
            </CodeBlock>
          </div>
        </div>
      </div>
    </section>
  );
}

function BlazorComparisonSection() {
  return (
    <section className="blazor-comparison">
      <div className="container">
        <h2>How Does It Compare?</h2>
        <p className="blazor-comparison__intro">
          Blazor and Tombatron.Turbo both bring rich interactivity to .NET
          developers, but with different trade-offs. Pick the approach that fits
          your project.
        </p>
        <div className="blazor-comparison__grid">
          <div className="blazor-comparison__card">
            <h3>Blazor Server</h3>
            <ul>
              <li>Persistent SignalR circuit per user</li>
              <li>Stateful components with C# logic</li>
              <li>Small download, higher server memory</li>
              <li>Every interaction round-trips to server</li>
            </ul>
          </div>
          <div className="blazor-comparison__card">
            <h3>Blazor WASM</h3>
            <ul>
              <li>.NET runtime in WebAssembly</li>
              <li>Offline-capable after first load</li>
              <li>Large initial download (~5-10 MB)</li>
              <li>Full C# execution in the browser</li>
            </ul>
          </div>
          <div className="blazor-comparison__card blazor-comparison__card--highlight">
            <h3>Tombatron.Turbo</h3>
            <ul>
              <li>Stateless HTTP + targeted DOM updates</li>
              <li>~30 KB JS layer, no .NET in browser</li>
              <li>Progressive enhancement built in</li>
              <li>Standard Razor — no new component model</li>
            </ul>
          </div>
        </div>
        <div className="blazor-comparison__cta">
          <Link to="/docs/migration/from-blazor-server">
            Read the Blazor migration guide &rarr;
          </Link>
        </div>
      </div>
    </section>
  );
}

function CTASection() {
  return (
    <section className="cta-section">
      <h2>Ready to try a different approach?</h2>
      <p>
        Add one NuGet package to your existing ASP.NET Core app and start
        shipping faster, lighter web experiences today.
      </p>
      <Link className="hero__button--primary" to="/docs/getting-started">
        Read the Quickstart &rarr;
      </Link>
      <div className="cta__install">
        <code>dotnet add package Tombatron.Turbo</code>
      </div>
    </section>
  );
}

export default function Home(): React.JSX.Element {
  const {siteConfig} = useDocusaurusContext();
  return (
    <Layout
      title="Hotwire Turbo for ASP.NET Core"
      description="Build fast, modern web apps with server-rendered HTML and SignalR-powered real-time streams. No JavaScript framework needed.">
      <HeroSection />
      <main>
        <PitchBar />
        <FeaturesSection />
        <CodeShowcase />
        <BlazorComparisonSection />
        <CTASection />
      </main>
    </Layout>
  );
}
